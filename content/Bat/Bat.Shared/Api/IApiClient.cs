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

	public const string API_ENDPOINT_USERS_ME = "/api/users/-me";
	public const string API_ENDPOINT_USERS_ME_PROFILE = "/api/users/-me/profile";
	public const string API_ENDPOINT_USERS_ME_PASSWORD = "/api/users/-me/password";

	public const string API_ENDPOINT_CLAIMS = "/api/claims";
	public const string API_ENDPOINT_ROLES = "/api/roles";
	public const string API_ENDPOINT_ROLES_ID = "/api/roles/{id}";

	public const string API_ENDPOINT_APPS = "/api/apps";

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_INFO"/> to get the backend information.
	/// </summary>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<InfoResp>> InfoAsync(string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_AUTH_SIGNIN"/> to sign in a user.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AuthResp>> LoginAsync(AuthReq req, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_AUTH_REFRESH"/> to refresh current user's authentication token.
	/// </summary>
	/// <param name="authToken">The authentication token to authenticate current user.</param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<AuthResp>> RefreshAsync(string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_USERS_ME"/> to get current user's information.
	/// </summary>
	/// <param name="authToken">The authentication token to authenticate current user.</param>
	/// <param name="baseUrl">The base URL of the API, optional.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for the API call, optional.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<UserResp>> GetMyInfoAsync(string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_USERS_ME_PROFILE"/> to update current user's profile.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<UserResp>> UpdateMyProfileAsync(UpdateUserProfileReq req, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_USERS_ME_PASSWORD"/> to change current user's password.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<ChangePasswordResp>> ChangeMyPasswordAsync(ChangePasswordReq req, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_CLAIMS"/> to get all claims.
	/// </summary>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<IEnumerable<ClaimResp>>> GetAllClaimsAsync(string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_ROLES"/> to get all roles.
	/// </summary>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<IEnumerable<RoleResp>>> GetAllRolesAsync(string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_ROLES"/> to create a new role.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<RoleResp>> CreateRoleAsync(CreateOrUpdateRoleReq req, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_ROLES_ID"/> to get a role by id.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<RoleResp>> GetRoleAsync(string id, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_ROLES_ID"/> to delete a role by id.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<RoleResp>> DeleteRoleAsync(string id, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Calls the API <see cref="API_ENDPOINT_ROLES_ID"/> to update a role.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="req"></param>
	/// <param name="authToken"></param>
	/// <param name="baseUrl"></param>
	/// <param name="httpClient"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<ApiResp<RoleResp>> UpdateRoleAsync(string id, CreateOrUpdateRoleReq req, string authToken, string? baseUrl = default, HttpClient? httpClient = default, CancellationToken cancellationToken = default);
}
