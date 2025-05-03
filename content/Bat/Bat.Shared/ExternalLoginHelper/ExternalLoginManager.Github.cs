using System.Text;
using System.Text.Json;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Bat.Shared.ExternalLoginHelper;

// https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/authorizing-oauth-apps
public sealed partial class ExternalLoginManager
{
	// https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/scopes-for-oauth-apps#available-scopes
	static readonly ISet<string> GH_DEFAULT_SCOPES = new HashSet<string> { "user:email", "read:user" };

	const string GH_PROVIDER_NAME = "GitHub";
	const string GH_CONF_CLIENT_ID = "ClientId";
	const string GH_CONF_PROMPT = "Prompt";
	const string GH_CONF_CLIENT_SECRET = "ClientSecret";
	const string GH_URL_PARAM_CODE = "code";
	const string GH_URL_PARAM_STATE = "state";
	const string GH_URL_PARAM_REDIRECT_URI = "redirect_uri";

	private string BuildAuthenticationUrlGitHub(ExternalLoginProviderConfig providerConfig, BuildAuthUrlReq req)
	{
		var clientId = providerConfig.GetValueOrDefault(GH_CONF_CLIENT_ID, string.Empty);
		var prompt = providerConfig.GetValueOrDefault(GH_CONF_PROMPT, "select_account");
		var state = string.IsNullOrEmpty(req.State) ? Guid.NewGuid().ToString("N") : req.State;
		var scopes = req.Scopes ?? new HashSet<string>();
		scopes.UnionWith(GH_DEFAULT_SCOPES);

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

		var result = QueryHelpers.AddQueryString("https://github.com/login/oauth/authorize", new Dictionary<string, string>
		{
			{ "client_id", clientId },
			{ "redirect_uri", redirectUri },
			{ "scope", string.Join(' ', scopes) },
			{ "state", state },
			{ "prompt", prompt },
		});
		Logger.LogDebug("BuildAuthenticationUrlGitHub({providerConfig}, {req}): {result}",
			JsonSerializer.Serialize(providerConfig), JsonSerializer.Serialize(req),
			result);
		return result;
	}

	private async Task<ExternalLoginResult> AuthenticateGitHubAsync(ExternalLoginProviderConfig providerConfig, IDictionary<string, string> authReq)
	{
		var stateUtf8 = authReq.TryGetValue(GH_URL_PARAM_STATE, out var stateStr) ? Convert.FromBase64String(stateStr) : Encoding.UTF8.GetBytes("{}");
		var stateData = JsonSerializer.Deserialize<Dictionary<string, object>>(stateUtf8) ?? [];

		var clientId = providerConfig.GetValueOrDefault(GH_CONF_CLIENT_ID, string.Empty);
		var clientSecret = providerConfig.GetValueOrDefault(GH_CONF_CLIENT_SECRET, string.Empty);
		authReq.TryGetValue(GH_URL_PARAM_CODE, out var code);
		authReq.TryGetValue(GH_URL_PARAM_REDIRECT_URI, out var redirectUri);

		var tokenResult = new ExternalLoginResult();
		using var tokenReq = BuildHttpRequestMessage(HttpMethod.Post, $"https://github.com/login/oauth/access_token",
			headers: new Dictionary<string, string>
			{
				{ "Accept", "application/json" }
			},
			content: new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{ "client_id", clientId },
				{ "client_secret", clientSecret },
				{ "code", code ?? string.Empty},
				{ "redirect_uri", redirectUri ?? string.Empty },
			})
		);
		var tokenResp = await HttpClientHelper.HttpRequestThatReturnsJson(HttpClient, tokenReq);
		tokenResult.StatusCode = tokenResp.StatusCode;
		tokenResult.Provider = GH_PROVIDER_NAME;
		if (tokenResp.Error != null)
		{
			tokenResult.ErrorType = tokenResp.Error.GetType().Name;
			tokenResult.ErrorMessage = tokenResp.Error.Message;
		}
		else
		{
			tokenResult.ErrorType = tokenResp.ContentJson!.RootElement.TryGetProperty("error", out var element) ? element.GetString() : null;
			tokenResult.ErrorMessage = tokenResp.ContentJson!.RootElement.TryGetProperty("error_description", out element) ? element.GetString() : null;
			if (!tokenResp.IsSuccessStatusCode || !string.IsNullOrEmpty(tokenResult.ErrorType))
			{
				tokenResult.StatusCode = tokenResult.IsSuccessStatusCode ? 0 : tokenResp.StatusCode;
			}
			else
			{
				tokenResult.TokenType = tokenResp.ContentJson!.RootElement.TryGetProperty("token_type", out element) ? element.GetString() : null;
				tokenResult.Scope = tokenResp.ContentJson!.RootElement.TryGetProperty("scope", out element) ? element.GetString() : null;
				tokenResult.AccessToken = tokenResp.ContentJson!.RootElement.TryGetProperty("access_token", out element) ? element.GetString() : null;

				if (stateData.TryGetValue("returnUrl", out var returnUrl))
				{
					tokenResult.RedirectUrl = returnUrl.ToString();
				}
			}
		}

		Logger.LogDebug("AuthenticateGitHubAsync({providerConfig}, {authReq}): {tokenResult}",
			JsonSerializer.Serialize(providerConfig), JsonSerializer.Serialize(authReq),
			JsonSerializer.Serialize(tokenResult));

		return tokenResult;
	}

	private async Task<ExternalUserProfile> GetUserProfileGitHubAsync(string accessToken)
	{
		var profileResult = new ExternalUserProfile();
		using var profileReq = BuildHttpRequestMessage(HttpMethod.Get, "https://api.github.com/user",
			headers: new Dictionary<string, string>
			{
				{ "Authorization", $"Bearer {accessToken}" },
				{ "Accept", "application/json" },
				{ "User-Agent", "Blazor-Admin-Template" },
			}
		);
		var profileResp = await HttpClientHelper.HttpRequestThatReturnsJson(HttpClient, profileReq);
		profileResult.StatusCode = profileResp.StatusCode;
		profileResult.Provider = GH_PROVIDER_NAME;
		if (profileResp.Error != null)
		{
			profileResult.ErrorType = profileResp.Error.GetType().Name;
			profileResult.ErrorMessage = profileResp.Error.Message;
		}
		else if (!profileResp.IsSuccessStatusCode)
		{
			profileResult.ErrorType = profileResp.ContentJson!.RootElement.TryGetProperty("status", out var element) ? element.GetString() : null;
			profileResult.ErrorMessage = profileResp.ContentJson!.RootElement.TryGetProperty("message", out element) ? element.GetString() : null;
		}
		else
		{
			Logger.LogCritical("{log}", profileResp.Content);

			profileResult.Id = profileResp.ContentJson!.RootElement.TryGetProperty("login", out var element) ? element.GetString() : null;
			profileResult.GivenName = profileResp.ContentJson!.RootElement.TryGetProperty("name", out element) ? element.GetString() : null;
			profileResult.FamilyName = string.Empty;
			profileResult.DisplayName = profileResult.GivenName;
			profileResult.Email = profileResp.ContentJson!.RootElement.TryGetProperty("email", out element)
				? element.GetString()
				: profileResp.ContentJson!.RootElement.TryGetProperty("notification_email", out element) ? element.GetString() : null;
		}

		Logger.LogDebug("GetUserProfileGitHubAsync({accessToken}): {profileResult}",
			accessToken,
			JsonSerializer.Serialize(profileResult));

		return profileResult;
	}
}
