using System.Text.Json.Serialization;

namespace Bat.Shared.Api;

public interface IApiClient
{
	public const string API_ENDPOINT_HEALTH = "/health";
	public const string API_ENDPOINT_HEALTHZ = "/healthz";
	public const string API_ENDPOINT_READY = "/ready";
	public const string API_ENDPOINT_INFO = "/info";

	public const string API_ENDPOINT_AUTH_SIGNIN = "/auth/signin";
	public const string API_ENDPOINT_AUTH_LOGIN = "/auth/login";
	public const string API_ENDPOINT_AUTH_REFRESH = "/auth/refresh";

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_INFO"/>.
	/// </summary>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<InfoResp>> InfoAsync(string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_AUTH_SIGNIN"/>
	/// </summary>
	/// <param name="req"></param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AuthResp>> LoginAsync(LoginReq req, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_AUTH_REFRESH"/>.
	/// </summary>
	/// <param name="authToken">The authentication token to authenticate current user.</param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AuthResp>> RefreshAsync(string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);
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
