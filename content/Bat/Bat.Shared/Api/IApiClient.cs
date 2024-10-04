using System.Text.Json.Serialization;

namespace Bat.Shared.Api;
public interface IApiClient
{
	/// <summary>
	/// Calls the <c>/info</c> API.
	/// </summary>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<InfoResp>> InfoAsync(string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the <c>/auth</c> API.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AuthResp>> LoginAsync(LoginReq req, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request info for the <see cref="IApiClient.LoginAsync(LoginReq, string?, HttpClient?)"/> API call.
/// </summary>
public class LoginReq
{
	[JsonPropertyName("email")]
	public string? Email { get; set; }

	[JsonPropertyName("password")]
	public string? Password { get; set; }
}
