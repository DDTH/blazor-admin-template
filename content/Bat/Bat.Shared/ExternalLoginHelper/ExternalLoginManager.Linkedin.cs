using System.Text;
using System.Text.Json;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Bat.Shared.ExternalLoginHelper;

public sealed partial class ExternalLoginManager
{
	static readonly ISet<string> LINKEDIN_DEFAULT_SCOPES = new HashSet<string> { "openid", "email", "profile" };

	const string LINKEDIN_PROVIDER_NAME = "LinkedIn";
	const string LINKEDIN_CONF_CLIENT_ID = "ClientId";
	const string LINKEDIN_CONF_CLIENT_SECRET = "ClientSecret";
	const string LINKEDIN_URL_PARAM_CODE = "code";
	const string LINKEDIN_URL_PARAM_STATE = "state";
	const string LINKEDIN_URL_PARAM_REDIRECT_URI = "redirect_uri";

	private string BuildAuthenticationUrlLinkedIn(ExternalLoginProviderConfig providerConfig, BuildAuthUrlReq req)
	{
		var clientId = providerConfig.GetValueOrDefault(LINKEDIN_CONF_CLIENT_ID, string.Empty);
		var state = string.IsNullOrEmpty(req.State) ? Guid.NewGuid().ToString("N") : req.State;
		var scopes = req.Scopes ?? new HashSet<string>();
		scopes.UnionWith(LINKEDIN_DEFAULT_SCOPES);

		// encode all parameters and the state into the new "state" parameter
		var uri = new Uri(req.RedirectUrl);
		var queryParams = QueryHelpers.ParseQuery(uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped));
		var encodedStates = new Dictionary<string, object>();
		foreach (var (key, value) in queryParams)
		{
			encodedStates[key] = value.FirstOrDefault() ?? string.Empty;
		}
		encodedStates["__state"] = state;
		var jsStr = JsonSerializer.Serialize(encodedStates);
		state = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsStr));
		var redirectUri = uri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);

		var result = QueryHelpers.AddQueryString($"https://www.linkedin.com/oauth/v2/authorization", new Dictionary<string, string>
		{
			{ "client_id", clientId },
			{ "response_type", "code" },
			{ "redirect_uri", redirectUri },
			{ "scope", string.Join(' ', scopes) },
			{ "state", state },
		});
		Logger.LogDebug("BuildAuthenticationUrlLinkedIn({providerConfig}, {req}): {result}",
			JsonSerializer.Serialize(providerConfig), JsonSerializer.Serialize(req),
			result);
		return result;
	}

	private async Task<ExternalLoginResult> AuthenticateLinkedInAsync(ExternalLoginProviderConfig providerConfig, IDictionary<string, string> authReq)
	{
		var stateUtf8 = authReq.TryGetValue(LINKEDIN_URL_PARAM_STATE, out var stateStr) ? Convert.FromBase64String(stateStr) : Encoding.UTF8.GetBytes("{}");
		var stateData = JsonSerializer.Deserialize<Dictionary<string, object>>(stateUtf8) ?? [];

		var clientId = providerConfig.GetValueOrDefault(LINKEDIN_CONF_CLIENT_ID, string.Empty);
		var clientSecret = providerConfig.GetValueOrDefault(LINKEDIN_CONF_CLIENT_SECRET, string.Empty);
		authReq.TryGetValue(LINKEDIN_URL_PARAM_CODE, out var code);
		authReq.TryGetValue(LINKEDIN_URL_PARAM_REDIRECT_URI, out var redirectUri);

		var tokenResult = new ExternalLoginResult();
		using var tokenReq = BuildHttpRequestMessage(HttpMethod.Post, $"https://www.linkedin.com/oauth/v2/accessToken",
			content: new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{ "client_id", clientId },
				{ "grant_type", "authorization_code" },
				{ "code", code ?? string.Empty},
				{ "redirect_uri", redirectUri ?? string.Empty },
				{ "client_secret", clientSecret },
			})
		);
		var tokenResp = await HttpClientHelper.HttpRequestThatReturnsJson(HttpClient, tokenReq);
		tokenResult.StatusCode = tokenResp.StatusCode;
		tokenResult.Provider = LINKEDIN_PROVIDER_NAME;
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
			tokenResult.AccessToken = tokenResp.ContentJson!.RootElement.TryGetProperty("access_token", out element) ? element.GetString() : null;
			tokenResult.ExpireIn = tokenResp.ContentJson!.RootElement.TryGetProperty("expires_in", out element) ? element.GetInt32() : 0;
			tokenResult.ExpireAt = DateTimeOffset.UtcNow.AddSeconds(tokenResult.ExpireIn);

			tokenResult.RefreshToken = tokenResp.ContentJson!.RootElement.TryGetProperty("refresh_token", out element) ? element.GetString() : null;
			tokenResult.RefreshTokenExpireIn = tokenResp.ContentJson!.RootElement.TryGetProperty("refresh_token_expires_in", out element) ? element.GetInt32() : 0;
			tokenResult.RefreshTokenExpireAt = DateTimeOffset.UtcNow.AddSeconds(tokenResult.RefreshTokenExpireIn);

			tokenResult.Scope = tokenResp.ContentJson!.RootElement.TryGetProperty("scope", out element) ? element.GetString() : null;

			if (stateData.TryGetValue("returnUrl", out var returnUrl))
			{
				tokenResult.RedirectUrl = returnUrl.ToString();
			}
		}

		Logger.LogDebug("AuthenticateLinkedInAsync({providerConfig}, {authReq}): {tokenResult}",
			JsonSerializer.Serialize(providerConfig), JsonSerializer.Serialize(authReq),
			JsonSerializer.Serialize(tokenResult));

		return tokenResult;
	}

	private async Task<ExternalUserProfile> GetUserProfileLinkedInAsync(string accessToken)
	{
		var profileResult = new ExternalUserProfile();
		using var profileReq = BuildHttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo",
			headers: new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {accessToken}" }
			}
		);
		var profileResp = await HttpClientHelper.HttpRequestThatReturnsJson(HttpClient, profileReq);
		profileResult.StatusCode = profileResp.StatusCode;
		profileResult.Provider = LINKEDIN_PROVIDER_NAME;
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
			profileResult.Id = profileResp.ContentJson!.RootElement.TryGetProperty("sub", out var element) ? element.GetString() : null;
			profileResult.GivenName = profileResp.ContentJson!.RootElement.TryGetProperty("given_name", out element) ? element.GetString() : null;
			profileResult.FamilyName = profileResp.ContentJson!.RootElement.TryGetProperty("family_name", out element) ? element.GetString() : null;
			profileResult.DisplayName = profileResp.ContentJson!.RootElement.TryGetProperty("name", out element) ? element.GetString() : null;
			profileResult.Email = profileResp.ContentJson!.RootElement.TryGetProperty("email", out element) ? element.GetString() : null;
		}

		Logger.LogDebug("GetUserProfileLinkedInAsync({accessToken}): {profileResult}",
			accessToken,
			JsonSerializer.Serialize(profileResult));

		return profileResult;
	}
}
