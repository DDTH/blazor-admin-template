namespace Bat.Shared.Api;
public interface IApiClient
{
	/// <summary>
	/// Calls the <c>/info</c> API.
	/// </summary>
	/// <param name="baseUrl">The base URL of the API.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call.</param>
	/// <returns></returns>
	public Task<ApiResp<InfoResp>> InfoAsync(string? baseUrl = default, HttpClient? httpClient = default);
}
