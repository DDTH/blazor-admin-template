using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Bat.Shared.ExternalLoginHelper;

public sealed partial class ExternalLoginManager
{
	const string MS_DEFAULT_TENANT = "common";
	static readonly ISet<string> MS_DEFAULT_SCOPES = new HashSet<string> { "offline_access", "User.Read" };

	static string BuildAuthenticationUrlMicrosoft(ExternalLoginProviderConfig providerConfig, BuildAuthUrlReq req)
	{
		var tenantId = providerConfig.GetValueOrDefault("TenantId", MS_DEFAULT_TENANT);
		var clientId = providerConfig.GetValueOrDefault("ClientId", string.Empty);
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

		return QueryHelpers.AddQueryString($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize", new Dictionary<string, string>
		{
			{ "client_id", clientId },
			{ "response_type", "code" },
			{ "redirect_uri", redirectUri },
			{ "scope", string.Join(' ', scopes) },
			{ "response_mode", "query" },
			{ "state", state },
		});
	}

	async Task<ExternalLoginResult> AuthenticateMicrosoftAsync(ExternalLoginProviderConfig providerConfig, IDictionary<string, string> authReq)
	{
		var stateUtf8 = authReq.TryGetValue("state", out var stateStr) ? Convert.FromBase64String(stateStr) : Encoding.UTF8.GetBytes("{}");
		var stateData = JsonSerializer.Deserialize<Dictionary<string, object>>(stateUtf8) ?? [];

		// scopes are encoded in state data
		var scopes = stateData.TryGetValue("__scopes", out var scopesObj) ? ((JsonElement)scopesObj).Deserialize<ISet<string>>() ?? new HashSet<string>() : new HashSet<string>();

		var tenantId = providerConfig.GetValueOrDefault("TenantId", MS_DEFAULT_TENANT);
		var clientId = providerConfig.GetValueOrDefault("ClientId", string.Empty);
		var clientSecret = providerConfig.GetValueOrDefault("ClientSecret", string.Empty);
		authReq.TryGetValue("code", out var code);
		authReq.TryGetValue("redirect_uri", out var redirectUri);

		var tokenResult = new ExternalLoginResult();
		var tokenReq = new HttpRequestMessage(HttpMethod.Post, $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token")
		{
			Content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{ "client_id", clientId },
				{ "grant_type", "authorization_code" },
				{ "scope", string.Join(' ', scopes) },
				{ "code", code ?? string.Empty},
				{ "redirect_uri", redirectUri ?? string.Empty },
				{ "client_secret", clientSecret },
			}),
		};
		var tokenResp = await HttpClientHelper.HttpRequestThatReturnsJson(HttpClient, tokenReq);
		tokenResult.StatusCode = tokenResp.StatusCode;
		tokenResult.Provider = "Microsoft";
		if (tokenResp.Error != null)
		{
			tokenResult.ErrorType = tokenResp.Error.GetType().Name;
			tokenResult.ErrorMessage = tokenResp.Error.Message;
		}
		else if (!tokenResp.IsSuccessStatusCode)
		{
			tokenResult.ErrorType = tokenResp.ContentJson?.RootElement.GetProperty("error").GetString();
			tokenResult.ErrorMessage = tokenResp.ContentJson?.RootElement.GetProperty("error_description").GetString();
		}
		else
		{
			tokenResult.TokenType = tokenResp.ContentJson?.RootElement.GetProperty("token_type").GetString();
			tokenResult.Scope = tokenResp.ContentJson?.RootElement.GetProperty("scope").GetString();
			tokenResult.AccessToken = tokenResp.ContentJson?.RootElement.GetProperty("access_token").GetString();
			tokenResult.RefreshToken = tokenResp.ContentJson?.RootElement.GetProperty("refresh_token").GetString();
			tokenResult.ExpireIn = tokenResp.ContentJson?.RootElement.GetProperty("expires_in").GetInt32() ?? 0;
			tokenResult.ExpireAt = DateTimeOffset.Now.AddSeconds(tokenResult.ExpireIn);
			if (stateData.TryGetValue("returnUrl", out var returnUrl))
			{
				tokenResult.RedirectUrl = returnUrl.ToString();
			}
		}

		return tokenResult;
	}

	async Task<ExternalUserProfile> GetUserProfileMicrosoftAsync(string accessToken)
	{
		var profileResult = new ExternalUserProfile();
		var profileReq = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")
		{
			Headers =
			{
				Authorization = new AuthenticationHeaderValue("Bearer", accessToken),
			},
		};
		var profileResp = await HttpClientHelper.HttpRequestThatReturnsJson(HttpClient, profileReq);
		profileResult.StatusCode = profileResp.StatusCode;
		profileResult.Provider = "Microsoft";
		if (profileResp.Error != null)
		{
			profileResult.ErrorType = profileResp.Error.GetType().Name;
			profileResult.ErrorMessage = profileResp.Error.Message;
		}
		else if (!profileResp.IsSuccessStatusCode)
		{
			profileResult.ErrorType = profileResp.ContentJson?.RootElement.GetProperty("error").GetString();
			profileResult.ErrorMessage = profileResp.ContentJson?.RootElement.GetProperty("error_description").GetString();
		}
		else
		{
			profileResult.Id = profileResp.ContentJson?.RootElement.GetProperty("id").GetString();
			profileResult.GivenName = profileResp.ContentJson?.RootElement.GetProperty("givenName").GetString();
			profileResult.FamilyName = profileResp.ContentJson?.RootElement.GetProperty("surname").GetString();
			profileResult.DisplayName = profileResp.ContentJson?.RootElement.GetProperty("displayName").GetString();
			profileResult.Email = profileResp.ContentJson?.RootElement.GetProperty("mail").GetString()
				?? profileResp.ContentJson?.RootElement.GetProperty("userPrincipalName").GetString();
		}

		return profileResult;
	}
}
