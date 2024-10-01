using Bat.Shared.Api;
using System.Net.Http.Json;

namespace Bat.Blazor.Client.Services;

/// <summary>
/// The <see cref="IApiClient"/> implementation that uses <see cref="HttpClient"/> to make API calls to the remote server.
/// </summary>
/// <param name="httpClient"></param>
public class HttpApiClient(HttpClient httpClient) : IApiClient
{
	//private readonly ApiResp ApiRespInvalidResponseFromServer = new() { Status = 500, Message = "Invalid response from server." };

	/// <inheritdoc/>
	public async Task<ApiResp<InfoResp>> InfoAsync()
	{
		var result = await httpClient.GetFromJsonAsync<ApiResp<InfoResp>>($"{Globals.ApiBaseUrl}/info");
		return result ?? new ApiResp<InfoResp> { Status = 500, Message = "Invalid response from server." };
	}
}
