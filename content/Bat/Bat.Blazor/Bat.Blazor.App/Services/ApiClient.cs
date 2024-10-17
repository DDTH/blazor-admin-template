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

	private static HttpRequestMessage BuildRequest(HttpMethod method, Uri endpoint, string? authToken)
	{
		return BuildRequest(method, endpoint, authToken, null);
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
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), IApiClient.API_ENDPOINT_AUTH_SIGNIN);
		var httpReq = BuildRequest(HttpMethod.Post, apiUri, null, req);
		var httpResult = await usingHttpClient.SendAsync(httpReq, cancellationToken);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<AuthResp>>(cancellationToken);
		return result ?? new ApiResp<AuthResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AuthResp>> RefreshAsync(string authToken, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), IApiClient.API_ENDPOINT_AUTH_REFRESH);
		var httpReq = BuildRequest(HttpMethod.Post, apiUri, authToken);
		var httpResult = await usingHttpClient.SendAsync(httpReq, cancellationToken);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<AuthResp>>(cancellationToken);
		return result ?? new ApiResp<AuthResp> { Status = 500, Message = "Invalid response from server." };
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<UserResp>> GetMyInfoAsync(string authToken, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), IApiClient.API_ENDPOINT_USERS_ME);
		var httpReq = BuildRequest(HttpMethod.Get, apiUri, authToken);
		var httpResult = await usingHttpClient.SendAsync(httpReq, cancellationToken);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<UserResp>>(cancellationToken);
		return result ?? new ApiResp<UserResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<UserResp>> UpdateMyProfileAsync(UpdateUserProfileReq req, string authToken, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
	{
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), IApiClient.API_ENDPOINT_USERS_ME_PROFILE);
		var httpReq = BuildRequest(HttpMethod.Post, apiUri, authToken, req);
		var httpResult = await usingHttpClient.SendAsync(httpReq, cancellationToken);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<UserResp>>(cancellationToken);
		return result ?? new ApiResp<UserResp> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<ChangePasswordResp>> ChangeMyPasswordAsync(ChangePasswordReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), IApiClient.API_ENDPOINT_USERS_ME_PASSWORD);
		var httpReq = BuildRequest(HttpMethod.Post, apiUri, authToken, req);
		var httpResult = await usingHttpClient.SendAsync(httpReq, cancellationToken);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<ChangePasswordResp>>(cancellationToken);
		return result ?? new ApiResp<ChangePasswordResp> { Status = 500, Message = "Invalid response from server." };
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<ApiResp<IEnumerable<ClaimResp>>> GetAllClaimsAsync(string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), IApiClient.API_ENDPOINT_CLAIMS);
		var httpReq = BuildRequest(HttpMethod.Get, apiUri, null);
		var httpResult = await usingHttpClient.SendAsync(httpReq, cancellationToken);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<IEnumerable<ClaimResp>>>(cancellationToken);
		return result ?? new ApiResp<IEnumerable<ClaimResp>> { Status = 500, Message = "Invalid response from server." };
	}

	/// <inheritdoc/>
	public async Task<ApiResp<IEnumerable<RoleResp>>> GetAllRolesAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), IApiClient.API_ENDPOINT_ROLES);
		var httpReq = BuildRequest(HttpMethod.Get, apiUri, authToken);
		var httpResult = await usingHttpClient.SendAsync(httpReq, cancellationToken);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<IEnumerable<RoleResp>>>(cancellationToken);
		return result ?? new ApiResp<IEnumerable<RoleResp>> { Status = 500, Message = "Invalid response from server." };
	}
}
