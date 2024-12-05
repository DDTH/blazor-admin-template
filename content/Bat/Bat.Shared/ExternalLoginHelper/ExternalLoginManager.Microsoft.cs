using System.Text;
using System.Text.Json;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Bat.Shared.ExternalLoginHelper;

public sealed partial class ExternalLoginManager
{
	static readonly ISet<string> MS_DEFAULT_SCOPES = new HashSet<string> { "offline_access", "User.Read" };

	const string MS_DEFAULT_TENANT = "common";
	const string MS_PROVIDER_NAME = "Microsoft";
	const string MS_CONF_TENANT_ID = "TenantId";
	const string MS_CONF_CLIENT_ID = "ClientId";
	const string MS_CONF_CLIENT_SECRET = "ClientSecret";
	const string MS_URL_PARAM_CODE = "code";
	const string MS_URL_PARAM_STATE = "state";
	const string MS_URL_PARAM_REDIRECT_URI = "redirect_uri";

	private string BuildAuthenticationUrlMicrosoft(ExternalLoginProviderConfig providerConfig, BuildAuthUrlReq req)
	{
		var tenantId = providerConfig.GetValueOrDefault(MS_CONF_TENANT_ID, MS_DEFAULT_TENANT);
		var clientId = providerConfig.GetValueOrDefault(MS_CONF_CLIENT_ID, string.Empty);
		var state = string.IsNullOrEmpty(req.State) ? Guid.NewGuid().ToString("N") : req.State;
		var scopes = req.Scopes ?? new HashSet<string>();
		scopes.UnionWith(MS_DEFAULT_SCOPES);

		// encode all parameters and the state and scopes into the new "state" parameter
		var uri = new Uri(req.RedirectUrl);
		var queryParams = QueryHelpers.ParseQuery(uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped));
		var encodedStates = new Dictionary<string, object>();
		foreach (var (key, value) in queryParams)
		{
			encodedStates[key] = value.FirstOrDefault() ?? string.Empty;
		}
		encodedStates["__state"] = state;
		encodedStates["__scopes"] = scopes;
		var jsStr = JsonSerializer.Serialize(encodedStates);
		state = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsStr));
		var redirectUri = uri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);

		var result = QueryHelpers.AddQueryString($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize", new Dictionary<string, string>
		{
			{ "client_id", clientId },
			{ "response_type", "code" },
			{ "redirect_uri", redirectUri },
			{ "scope", string.Join(' ', scopes) },
			{ "response_mode", "query" },
			{ "state", state },
		});
		Logger.LogDebug("BuildAuthenticationUrlMicrosoft({providerConfig}, {req}): {result}",
			JsonSerializer.Serialize(providerConfig), JsonSerializer.Serialize(req),
			result);
		return result;
	}

	private async Task<ExternalLoginResult> AuthenticateMicrosoftAsync(ExternalLoginProviderConfig providerConfig, IDictionary<string, string> authReq)
	{
		var stateUtf8 = authReq.TryGetValue(MS_URL_PARAM_STATE, out var stateStr) ? Convert.FromBase64String(stateStr) : Encoding.UTF8.GetBytes("{}");
		var stateData = JsonSerializer.Deserialize<Dictionary<string, object>>(stateUtf8) ?? [];

		// scopes are encoded in state data
		var scopes = stateData.TryGetValue("__scopes", out var scopesObj) ? ((JsonElement)scopesObj).Deserialize<ISet<string>>() ?? new HashSet<string>() : new HashSet<string>();

		var tenantId = providerConfig.GetValueOrDefault(MS_CONF_TENANT_ID, MS_DEFAULT_TENANT);
		var clientId = providerConfig.GetValueOrDefault(MS_CONF_CLIENT_ID, string.Empty);
		var clientSecret = providerConfig.GetValueOrDefault(MS_CONF_CLIENT_SECRET, string.Empty);
		authReq.TryGetValue(MS_URL_PARAM_CODE, out var code);
		authReq.TryGetValue(MS_URL_PARAM_REDIRECT_URI, out var redirectUri);

		var tokenResult = new ExternalLoginResult();
		using var tokenReq = BuildHttpRequestMessage(HttpMethod.Post, $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
			content: new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{ "client_id", clientId },
				{ "grant_type", "authorization_code" },
				{ "scope", string.Join(' ', scopes) },
				{ "code", code ?? string.Empty},
				{ "redirect_uri", redirectUri ?? string.Empty },
				{ "client_secret", clientSecret },
			})
		);
		var tokenResp = await HttpClientHelper.HttpRequestThatReturnsJson(HttpClient, tokenReq);
		tokenResult.StatusCode = tokenResp.StatusCode;
		tokenResult.Provider = MS_PROVIDER_NAME;
		if (tokenResp.Error != null)
		{
			tokenResult.ErrorType = tokenResp.Error.GetType().Name;
			tokenResult.ErrorMessage = tokenResp.Error.Message;
		}
		else if (!tokenResp.IsSuccessStatusCode)
		{
			tokenResult.ErrorType = tokenResp.ContentJson!.RootElement.TryGetProperty("error", out var element) ? element.GetString() : null;
			tokenResult.ErrorMessage = tokenResp.ContentJson!.RootElement.TryGetProperty("error_description", out element) ? element.GetString() : null;
		}
		else
		{
			tokenResult.TokenType = tokenResp.ContentJson!.RootElement.TryGetProperty("token_type", out var element) ? element.GetString() : null;
			tokenResult.Scope = tokenResp.ContentJson!.RootElement.TryGetProperty("scope", out element) ? element.GetString() : null;
			tokenResult.AccessToken = tokenResp.ContentJson!.RootElement.TryGetProperty("access_token", out element) ? element.GetString() : null;
			tokenResult.RefreshToken = tokenResp.ContentJson!.RootElement.TryGetProperty("refresh_token", out element) ? element.GetString() : null;
			tokenResult.ExpireIn = tokenResp.ContentJson!.RootElement.TryGetProperty("expires_in", out element) ? element.GetInt32() : 0;
			tokenResult.ExpireAt = DateTimeOffset.Now.AddSeconds(tokenResult.ExpireIn);

			if (stateData.TryGetValue("returnUrl", out var returnUrl))
			{
				tokenResult.RedirectUrl = returnUrl.ToString();
			}
		}

		Logger.LogDebug("AuthenticateMicrosoftAsync({providerConfig}, {authReq}): {tokenResult}",
			JsonSerializer.Serialize(providerConfig), JsonSerializer.Serialize(authReq),
			JsonSerializer.Serialize(tokenResult));

		return tokenResult;
	}

	private async Task<ExternalUserProfile> GetUserProfileMicrosoftAsync(string accessToken)
	{
		var profileResult = new ExternalUserProfile();
		using var profileReq = BuildHttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me",
			headers: new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {accessToken}" }
			}
		);
		var profileResp = await HttpClientHelper.HttpRequestThatReturnsJson(HttpClient, profileReq);
		profileResult.StatusCode = profileResp.StatusCode;
		profileResult.Provider = MS_PROVIDER_NAME;
		if (profileResp.Error != null)
		{
			profileResult.ErrorType = profileResp.Error.GetType().Name;
			profileResult.ErrorMessage = profileResp.Error.Message;
		}
		else if (!profileResp.IsSuccessStatusCode)
		{
			profileResult.ErrorType = profileResp.ContentJson!.RootElement.TryGetProperty("error", out var element) ? element.GetString() : null;
			profileResult.ErrorMessage = profileResp.ContentJson!.RootElement.TryGetProperty("error_description", out element) ? element.GetString() : null;
		}
		else
		{
			profileResult.Id = profileResp.ContentJson!.RootElement.TryGetProperty("id", out var element) ? element.GetString() : null;
			profileResult.GivenName = profileResp.ContentJson!.RootElement.TryGetProperty("givenName", out element) ? element.GetString() : null;
			profileResult.FamilyName = profileResp.ContentJson!.RootElement.TryGetProperty("surname", out element) ? element.GetString() : null;
			profileResult.DisplayName = profileResp.ContentJson!.RootElement.TryGetProperty("displayName", out element) ? element.GetString() : null;
			profileResult.Email = profileResp.ContentJson!.RootElement.TryGetProperty("mail", out element)
				? element.GetString()
				: profileResp.ContentJson!.RootElement.TryGetProperty("userPrincipalName", out element) ? element.GetString() : null;
		}

		Logger.LogDebug("GetUserProfileMicrosoftAsync({accessToken}): {profileResult}",
			accessToken,
			JsonSerializer.Serialize(profileResult));

		return profileResult;
	}
}
