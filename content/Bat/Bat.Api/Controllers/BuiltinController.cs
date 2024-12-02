using Bat.Api.Services;
using Bat.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bat.Api.Controllers;

/// <summary>
/// Built-in controller that provides handlers for health check, info and sign-in.
/// </summary>
public class BuiltinController : ApiBaseController
{
	private readonly IConfiguration _conf;
	private readonly IWebHostEnvironment _env;
	private readonly IAuthenticator? _authenticator;
	private readonly IAuthenticatorAsync? _authenticatorAsync;

	public BuiltinController(
		IConfiguration config,
		IWebHostEnvironment env,
		IOptions<CryptoOptions> cryptoOptions,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync)
	{
		ArgumentNullException.ThrowIfNull(config, nameof(config));
		ArgumentNullException.ThrowIfNull(env, nameof(env));
		ArgumentNullException.ThrowIfNull(cryptoOptions, nameof(cryptoOptions));
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.", (Exception?)null);
		}

		_conf = config;
		_env = env;
		_authenticator = authenticator;
		_authenticatorAsync = authenticatorAsync;

		appInfo = new AppInfo
		{
			Name = _conf["App:Name"] ?? "",
			Version = _conf["App:Version"] ?? "",
			Description = _conf["App:Description"] ?? "",
		};
		cryptoInfo = new CryptoInfo
		{
			PubKey = Convert.ToBase64String(cryptoOptions.Value.RSAPubKey.ExportRSAPublicKey()),
			PubKeyType = "RSA-PKCS#1",
		};
	}

	/// <summary>
	/// Catch-all route for 404 errors.
	/// </summary>
	/// <returns></returns>
	[HttpGet("/api/{**slug}")]
	[HttpPost("/api/{**slug}")]
	[HttpPut("/api/{**slug}")]
	[HttpDelete("/api/{**slug}")]
	[HttpPatch("/api/{**slug}")]
	[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResp))]
	public ActionResult<ApiResp> _404()
	{
		return _respNotFound;
	}

	/// <summary>
	/// Checks if the server is running.
	/// </summary>
	/// <response code="200">Server is running.</response>
	[HttpGet(IApiClient.API_ENDPOINT_HEALTH)]
	[HttpGet(IApiClient.API_ENDPOINT_HEALTHZ)]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResp))]
	public ActionResult<ApiResp> Health() => ResponseOk();

	/// <summary>
	/// Checks if server is ready to handle requests.
	/// </summary>
	/// <response code="200">Server is ready to handle requests.</response>
	/// <response code="503">Server is running but NOT yet ready to handle requests.</response>
	[HttpGet(IApiClient.API_ENDPOINT_READY)]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResp<bool>))]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Type = typeof(ApiResp))]
	public ActionResult<ApiResp<bool>> Ready()
	{
		return Globals.Ready ? ResponseOk(true) : _respServiceUnavailable;
	}

	private readonly AppInfo appInfo;
	private readonly CryptoInfo cryptoInfo;

	/// <summary>
	/// Returns service's information.
	/// </summary>
	/// <response code="200">Server's information.</response>
	[HttpGet(IApiClient.API_ENDPOINT_INFO)]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResp<InfoResp>))]
	public ActionResult<ApiResp<InfoResp>> Info()
	{
		var data = new InfoResp
		{
			Ready = Globals.Ready,
			App = appInfo,
			Server = new ServerInfo
			{
				Env = _env.EnvironmentName,
				Time = DateTime.Now,
			},
			Crypto = cryptoInfo,
		};
		return ResponseOk(data);
	}

	/// <summary>
	/// Authenticates the client.
	/// </summary>
	/// <response code="200">Authentication was succesful.</response>
	/// <response code="403">Authentication failed.</response>
	/// <response code="500">No authenticator defined or error while authenticating.</response>
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResp<AuthResp>), StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	[HttpPost(IApiClient.API_ENDPOINT_AUTH_SIGNIN)]
	[HttpPost(IApiClient.API_ENDPOINT_AUTH_LOGIN)]
	public async Task<ActionResult<ApiResp<AuthResp>>> Authenticate([FromBody] AuthReq authReq)
	{
		ArgumentNullException.ThrowIfNull(authReq, nameof(authReq));

		var resp = _authenticatorAsync != null
			? await _authenticatorAsync.AuthenticateAsync(authReq)
			: _authenticator!.Authenticate(authReq);
		return resp.Status == 200
				? ResponseOk(resp)
				: ResponseNoData(resp.Status, resp.Error);
	}

	/// <summary>
	/// Refreshes the client's authentication token.
	/// </summary>
	/// <returns></returns>
	/// <response code="200">Authentication token was refreshed succesfully.</response>
	/// <response code="401">No authentication token found.</response>
	/// <response code="403">Invalid authentication token.</response>
	/// <response code="500">No authenticator defined or error while refreshing the token.</response>
	[HttpPost(IApiClient.API_ENDPOINT_AUTH_REFRESH)]
	[Authorize]
	public async Task<ActionResult<ApiResp<AuthResp>>> RefreshAuthToken()
	{
		var token = Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
		if (string.IsNullOrEmpty(token))
		{
			return ResponseNoData(401, "No auth token found.");
		}
		var resp = _authenticatorAsync != null
			? await _authenticatorAsync.RefreshAsync(token)
			: _authenticator!.Refresh(token);
		return resp.Status == 200
				? ResponseOk(resp)
				: ResponseNoData(resp.Status, resp.Error);
	}
}
