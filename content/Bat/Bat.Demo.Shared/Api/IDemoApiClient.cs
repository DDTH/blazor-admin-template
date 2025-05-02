using Bat.Shared.Api;

namespace Bat.Demo.Shared.Api;

public interface IDemoApiClient : IApiClient
{
	public const string API_ENDPOINT_APPS = "/api/apps";
	public const string API_ENDPOINT_APPS_ID = "/api/apps/{id}";

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_APPS"/> to get all applications.
	/// </summary>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<IEnumerable<AppResp>>> GetAllAppsAsync(string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_APPS"/> to create a new application.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AppResp>> CreateAppAsync(CreateOrUpdateAppReq req, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_APPS_ID"/> to get an application by id.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AppResp>> GetAppAsync(string id, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_APPS_ID"/> to delete an application by id.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AppResp>> DeleteAppAsync(string id, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_APPS_ID"/> to update an application.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="req"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AppResp>> UpdateAppAsync(string id, CreateOrUpdateAppReq req, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);
}
