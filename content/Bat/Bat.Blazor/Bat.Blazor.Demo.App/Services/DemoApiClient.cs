using Bat.Blazor.App.Services;
using Bat.Demo.Shared.Api;
using Bat.Shared.Api;

namespace Bat.Blazor.Demo.App.Services;

public class DemoApiClient : ApiClient, IDemoApiClient
{
	public DemoApiClient(HttpClient httpClient) : base(httpClient) { }

	/// <inheritdoc/>
	public async Task<ApiResp<IEnumerable<AppResp>>> GetAllAppsAsync(string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		using var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IDemoApiClient.API_ENDPOINT_APPS,
			authToken,
			NoData,
			cancellationToken
		);
		return await ReadAndCloseResponseAsync<IEnumerable<AppResp>>(httpResult, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AppResp>> CreateAppAsync(CreateOrUpdateAppReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		using var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Post, baseUrl, IDemoApiClient.API_ENDPOINT_APPS,
			authToken,
			req,
			cancellationToken
		);
		return await ReadAndCloseResponseAsync<AppResp>(httpResult, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AppResp>> GetAppAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		using var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Get, baseUrl, IDemoApiClient.API_ENDPOINT_APPS_ID.Replace("{id}", id, StringComparison.OrdinalIgnoreCase),
			authToken,
			NoData,
			cancellationToken
		);
		return await ReadAndCloseResponseAsync<AppResp>(httpResult, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AppResp>> DeleteAppAsync(string id, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		using var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Delete, baseUrl, IDemoApiClient.API_ENDPOINT_APPS_ID.Replace("{id}", id, StringComparison.OrdinalIgnoreCase),
			authToken,
			NoData,
			cancellationToken
		);
		return await ReadAndCloseResponseAsync<AppResp>(httpResult, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<ApiResp<AppResp>> UpdateAppAsync(string id, CreateOrUpdateAppReq req, string authToken, string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
	{
		using var httpResult = await BuildAndSendRequestAsync(
			requestHttpClient,
			HttpMethod.Put, baseUrl, IDemoApiClient.API_ENDPOINT_APPS_ID.Replace("{id}", id, StringComparison.OrdinalIgnoreCase),
			authToken,
			req,
			cancellationToken
		);
		return await ReadAndCloseResponseAsync<AppResp>(httpResult, cancellationToken);
	}
}
