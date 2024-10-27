using Bat.Shared.Api;
using System.Net.Http.Json;

namespace Bat.Blazor.App.Services;

public class ApiClient(HttpClient httpClient) : IApiClient
{
	private void UsingBaseUrlAndHttpClient(string? baseUrl, HttpClient? requestHttpClient, out string usingBaseUrl, out HttpClient usingHttpClient)
	{
		usingBaseUrl = string.IsNullOrEmpty(baseUrl) ? Globals.ApiBaseUrl ?? string.Empty : baseUrl;
		usingHttpClient = requestHttpClient ?? httpClient;
	}

	private static HttpRequestMessage BuildRequest(HttpMethod method, Uri endpoint, string? authToken, object? requestData)
	{
		var req = new HttpRequestMessage(method, endpoint)
		{
			Headers = {
				{ "Accept", "application/json" }
			}
		};
		if (!string.IsNullOrEmpty(authToken))
		{
			req.Headers.Add("Authorization", $"Bearer {authToken}");
		}
		if (requestData != null)
		{
			req.Content = JsonContent.Create(requestData);
		}
		return req;
	}

	private static readonly string NoAuth = string.Empty;
	private static readonly object?	NoData = null;

	private async Task<HttpResponseMessage> BuildAndSendRequestAsync(HttpClient? requestHttpClient, HttpMethod method, string? baseUrl, string apiEndpoint, string? authToken, object? requestData, CancellationToken cancellationToken)
	{
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), apiEndpoint);
		var httpReq = BuildRequest(method, apiUri, authToken, requestData);
		return await usingHttpClient.SendAsync(httpReq, cancellationToken);
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<InfoResp>> InfoAsync(string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), IApiClient.API_ENDPOINT_INFO);
		// simple GET request, we don't need to build a request message
		var result = await usingHttpClient.GetFromJsonAsync<ApiResp<InfoResp>>(apiUri, cancellationToken);
		return result ?? new ApiResp<InfoResp> { Status = 500, Message = "Invalid response from server." };
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<AuthResp>> LoginAsync(AuthReq req, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_AUTH_SIGNIN,
			NoAuth,
			req,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<AuthResp>>(cancellationToken);
		return result ?? new ApiResp<AuthResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AuthResp>> RefreshAsync(string authToken, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_AUTH_REFRESH,
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<AuthResp>>(cancellationToken);
		return result ?? new ApiResp<AuthResp> { Status = 500, Message = "Invalid response from server." };
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<UserResp>> GetMyInfoAsync(string authToken, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_USERS_ME,
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<UserResp>>(cancellationToken);
		return result ?? new ApiResp<UserResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<UserResp>> UpdateMyProfileAsync(UpdateUserProfileReq req, string authToken, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_USERS_ME_PROFILE,
			authToken,
			req,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<UserResp>>(cancellationToken);
		return result ?? new ApiResp<UserResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<ChangePasswordResp>> ChangeMyPasswordAsync(ChangePasswordReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_USERS_ME_PASSWORD,
			authToken,
			req,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<ChangePasswordResp>>(cancellationToken);
		return result ?? new ApiResp<ChangePasswordResp> { Status = 500, Message = "Invalid response from server." };
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<IEnumerable<UserResp>>> GetAllUsersAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_USERS,
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<IEnumerable<UserResp>>>(cancellationToken);
		return result ?? new ApiResp<IEnumerable<UserResp>> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<UserResp>> CreateUserAsync(CreateOrUpdateUserReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_USERS,
			authToken,
			req,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<UserResp>>(cancellationToken);
		return result ?? new ApiResp<UserResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<UserResp>> GetUserAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_USERS_ID.Replace("{id}", id),
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<UserResp>>(cancellationToken);
		return result ?? new ApiResp<UserResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<UserResp>> DeleteUserAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Delete, baseUrl, IApiClient.API_ENDPOINT_USERS_ID.Replace("{id}", id),
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<UserResp>>(cancellationToken);
		return result ?? new ApiResp<UserResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<UserResp>> UpdateUserAsync(string id, CreateOrUpdateUserReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Put, baseUrl, IApiClient.API_ENDPOINT_USERS_ID.Replace("{id}", id),
			authToken,
			req,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<UserResp>>(cancellationToken);
		return result ?? new ApiResp<UserResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<IEnumerable<ClaimResp>>> GetAllClaimsAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_CLAIMS,
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<IEnumerable<ClaimResp>>>(cancellationToken);
		return result ?? new ApiResp<IEnumerable<ClaimResp>> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<IEnumerable<RoleResp>>> GetAllRolesAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_ROLES,
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<IEnumerable<RoleResp>>>(cancellationToken);
		return result ?? new ApiResp<IEnumerable<RoleResp>> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<RoleResp>> CreateRoleAsync(CreateOrUpdateRoleReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Post, baseUrl, IApiClient.API_ENDPOINT_ROLES,
			authToken,
			req,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<RoleResp>>(cancellationToken);
		return result ?? new ApiResp<RoleResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<RoleResp>> GetRoleAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_ROLES_ID.Replace("{id}", id),
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<RoleResp>>(cancellationToken);
		return result ?? new ApiResp<RoleResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<RoleResp>> DeleteRoleAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Delete, baseUrl, IApiClient.API_ENDPOINT_ROLES_ID.Replace("{id}", id),
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<RoleResp>>(cancellationToken);
		return result ?? new ApiResp<RoleResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<RoleResp>> UpdateRoleAsync(string id, CreateOrUpdateRoleReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Put, baseUrl, IApiClient.API_ENDPOINT_ROLES_ID.Replace("{id}", id),
			authToken,
			req,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<RoleResp>>(cancellationToken);
		return result ?? new ApiResp<RoleResp> { Status = 500, Message = "Invalid response from server." };
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<IEnumerable<AppResp>>> GetAllAppsAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_APPS,
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<IEnumerable<AppResp>>>(cancellationToken);
		return result ?? new ApiResp<IEnumerable<AppResp>> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AppResp>> GetAppAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IApiClient.API_ENDPOINT_APPS_ID.Replace("{id}", id),
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<AppResp>>(cancellationToken);
		return result ?? new ApiResp<AppResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AppResp>> DeleteAppAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Delete, baseUrl, IApiClient.API_ENDPOINT_APPS_ID.Replace("{id}", id),
			authToken,
			NoData,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<AppResp>>(cancellationToken);
		return result ?? new ApiResp<AppResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AppResp>> UpdateAppAsync(string id, CreateOrUpdateAppReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Put, baseUrl, IApiClient.API_ENDPOINT_APPS_ID.Replace("{id}", id),
			authToken,
			req,
			cancellationToken
		);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<AppResp>>(cancellationToken);
		return result ?? new ApiResp<AppResp> { Status = 500, Message = "Invalid response from server." };
	}
}
