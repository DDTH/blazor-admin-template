using Bat.Shared.Api;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bat.Blazor.App.Services;

public class ApiClient(HttpClient httpClient) : IApiClient
{
    //private readonly ApiResp ApiRespInvalidResponseFromServer = new() { Status = 500, Message = "Invalid response from server." };

    private void UsingBaseUrlAndHttpClient(string? baseUrl, HttpClient? requestHttpClient, out string usingBaseUrl, out HttpClient usingHttpClient)
    {
        usingBaseUrl = string.IsNullOrEmpty(baseUrl) ? Globals.ApiBaseUrl??string.Empty:  baseUrl;
        usingHttpClient = requestHttpClient ?? httpClient;
    }

    /// <inheritdoc/>
    public async Task<ApiResp<InfoResp>> InfoAsync(string? baseUrl = default, HttpClient? requestHttpClient = default, CancellationToken cancellationToken = default)
    {
        UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
        var apiUri = new Uri(new Uri(usingBaseUrl), "/info");
        var result = await usingHttpClient.GetFromJsonAsync<ApiResp<InfoResp>>(apiUri, cancellationToken);
        return result ?? new ApiResp<InfoResp> { Status = 500, Message = "Invalid response from server." };
    }

    /// <inheritdoc/>
    public async Task<ApiResp<AuthResp>> LoginAsync(LoginReq req, string? baseUrl = null, HttpClient? requestHttpClient = null, CancellationToken cancellationToken = default)
    {
        UsingBaseUrlAndHttpClient(baseUrl, requestHttpClient, out var usingBaseUrl, out var usingHttpClient);
		var apiUri = new Uri(new Uri(usingBaseUrl), "/auth");
        var httpResult = await usingHttpClient.PostAsJsonAsync(apiUri, req, cancellationToken);
		var result = await httpResult.Content.ReadFromJsonAsync<ApiResp<AuthResp>>(cancellationToken);
		return result ?? new ApiResp<AuthResp> { Status = 500, Message = "Invalid response from server." };
	}
}
