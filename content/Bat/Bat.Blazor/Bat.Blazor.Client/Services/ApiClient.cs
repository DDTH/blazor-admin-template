using Bat.Blazor.App;
using Bat.Shared.Api;
using System.Net.Http.Json;

namespace Bat.Blazor.Client.Services;

public class ApiClient(HttpClient httpClient) : IApiClient
{
	//private readonly ApiResp ApiRespInvalidResponseFromServer = new() { Status = 500, Message = "Invalid response from server." };

	/// <inheritdoc/>
	public async Task<ApiResp<InfoResp>> InfoAsync(string? baseUrl = default, HttpClient? requestHttpClient = default)
	{
		var usingHttpClient = requestHttpClient ?? httpClient;
		var usingBaseUrl = string.IsNullOrEmpty(baseUrl) ? Globals.ApiBaseUrl : baseUrl;
		var apiUri = new Uri(new Uri(usingBaseUrl), "/info");
		var result = await usingHttpClient.GetFromJsonAsync<ApiResp<InfoResp>>(apiUri);
		return result ?? new ApiResp<InfoResp> { Status = 500, Message = "Invalid response from server." };
	}
}
