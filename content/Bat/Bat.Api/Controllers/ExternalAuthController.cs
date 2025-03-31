using System.Text.Json;
using Bat.Api.Services;
using Bat.Shared.Api;
using Bat.Shared.ExternalLoginHelper;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

public class ExternalAuthController : ApiBaseController
{
	private readonly IConfiguration _conf;
	private readonly IWebHostEnvironment _env;
	private readonly IAuthenticator? _authenticator;
	private readonly IAuthenticatorAsync? _authenticatorAsync;
	private readonly ExternalLoginManager _externalLoginManager;
	private readonly ILogger<ExternalAuthController> _logger;

	public ExternalAuthController(
		IConfiguration config,
		IWebHostEnvironment env,
		ExternalLoginManager externalLoginManager,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync,
		ILogger<ExternalAuthController> logger)
	{
		ArgumentNullException.ThrowIfNull(config, nameof(config));
		ArgumentNullException.ThrowIfNull(env, nameof(env));
		ArgumentNullException.ThrowIfNull(externalLoginManager, nameof(externalLoginManager));
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.", (Exception?)null);
		}

		_conf = config;
		_env = env;
		_externalLoginManager = externalLoginManager;
		_authenticator = authenticator;
		_authenticatorAsync = authenticatorAsync;
		_logger = logger;
	}

	/// <summary>
	/// Gets list of external authentication providers.
	/// </summary>
	[HttpGet("/api/external-auth/providers")]
	public ActionResult<ApiResp<IEnumerable<string>>> GetExternalAuthProviders()
	{
		var providers = _externalLoginManager.GetProviderNames() ?? [];
		_logger.LogDebug("GetExternalAuthProviders(): {providers}", JsonSerializer.Serialize(providers));
		return ResponseOk(providers);
	}

	/// <summary>
	/// Gets the authentication URL for the external  provider.
	/// </summary>
	/// <param name="request">The external authentication URL request.</param>
	/// <returns>The external authentication URL.</returns>
	[HttpPost("/api/external-auth/url")]
	public ActionResult<ApiResp<string>> GetExternalAuthUrl([FromBody] ExternalAuthUrlReq request)
	{
		try
		{
			var authUrl = _externalLoginManager.BuildAuthenticationUrl(request.Provider, new BuildAuthUrlReq
			{
				RedirectUrl = request.RedirectUrl,
			});
			_logger.LogDebug("GetExternalAuthUrl({request}): {authUrl}", JsonSerializer.Serialize(request), authUrl);
			return ResponseOk(authUrl);
		}
		catch (ExternalLoginException ex)
		{
			_logger.LogError(ex, "GetExternalAuthUrl({request}): {message}", JsonSerializer.Serialize(request), ex.Message);
			return ResponseNoData(500, ex.Message);
		}
	}

	/// <summary>
	/// Logs in using an external authentication provider.
	/// </summary>
	/// <param name="authReq">The external authentication request.</param>
	/// <param name="identityRepository"></param>
	/// <param name="lookupNormalizer"></param>
	/// <returns>The external authentication response.</returns>
	/// <exception cref="NoProviderException">Thrown when the provider is not found.</exception>
	/// <exception cref="ProviderNotSupported">Thrown when the provider is not supported.</exception>
	/// <exception cref="ExternalLoginException">Thrown when an error occurs while logging in.</exception>
	[HttpPost("/api/external-auth/login")]
	public async Task<ActionResult<ApiResp<ExternalAuthResp>>> ExternalLogin(
		[FromBody] ExternalAuthReq authReq,
		IIdentityRepository identityRepository,
		ILookupNormalizer lookupNormalizer)
	{
		try
		{
			// try to authenticate with external provider
			var loginResult = await _externalLoginManager.AuthenticateAsync(authReq.Provider, authReq.AuthData);
			if (!loginResult.IsSuccessStatusCode) return ResponseNoData(401, loginResult.ErrorMessage ?? loginResult.ErrorType ?? "Authentication failed.");
			_logger.LogDebug("ExternalLogin({authReq})/AuthenticateAsync: {loginResult}", JsonSerializer.Serialize(authReq), JsonSerializer.Serialize(loginResult));

			// get user profile from external provider
			var extProfileResult = await _externalLoginManager.GetUserProfileAsync(authReq.Provider, loginResult.AccessToken!);
			if (!extProfileResult.IsSuccessStatusCode) return ResponseNoData(401, extProfileResult.ErrorMessage ?? extProfileResult.ErrorType ?? "Failed to get user profile.");
			if (string.IsNullOrWhiteSpace(extProfileResult.Email)) return ResponseNoData(401, "User profile does not have a valid email address.");
			_logger.LogDebug("ExternalLogin({authReq})/GetUserProfileAsync: {extProfileResult}", JsonSerializer.Serialize(authReq), JsonSerializer.Serialize(extProfileResult));

			// create user account if needed
			var user = await EnsureUserAccountAsync(identityRepository, lookupNormalizer, extProfileResult);
			if (user is null) return ResponseNoData(500, "Failed to create user account.");

			// login user
			var resp = _authenticatorAsync != null
				? await _authenticatorAsync.AuthenticateAsync(extProfileResult)
				: _authenticator!.Authenticate(extProfileResult);
			if (resp.Status != 200) return ResponseNoData(resp.Status, resp.Error);
			var result = ExternalAuthResp.New(loginResult.Provider, status: 200, resp.Token!, resp.Expiry);
			result.ReturnUrl = loginResult.RedirectUrl;
			return ResponseOk(result);
		}
		catch (ExternalLoginException ex)
		{
			_logger.LogError(ex, "ExternalLogin({authReq}): {message}", JsonSerializer.Serialize(authReq), ex.Message);
			return ResponseNoData(500, ex.Message);
		}
	}

	struct SeedingRole
	{
		public string? Name { get; set; }
		public string? Description { get; set; }
		public IEnumerable<string>? Claims { get; set; }
	}

	private async Task<BatUser?> EnsureUserAccountAsync(IIdentityRepository identityRepository, ILookupNormalizer lookupNormalizer, ExternalUserProfile extProfile)
	{
		var user = await identityRepository.GetUserByEmailAsync(extProfile.Email!);
		if (user is not null) return user;

		// create user account
		var username = extProfile.Email!;
		var email = extProfile.Email!;
		user = new BatUser
		{
			UserName = username,
			NormalizedUserName = lookupNormalizer.NormalizeName(username),
			Email = email,
			NormalizedEmail = lookupNormalizer.NormalizeEmail(email),
			FamilyName = extProfile.FamilyName?.Trim() ?? extProfile.DisplayName?.Trim(),
			GivenName = extProfile.GivenName?.Trim() ?? extProfile.DisplayName?.Trim(),
		};
		var iresultCreate = await identityRepository.CreateAsync(user);
		if (!iresultCreate.Succeeded)
		{
			return null;
		}

		// add user to roles
		if (_env.IsDevelopment())
		{
			//FIXME for development/demo purposes only, NOT for production!
			var seedRoles = _conf.GetSection("SeedingData:Identity:Roles").Get<IEnumerable<SeedingRole>>() ?? [];
			var roles = seedRoles.Select(async sr => await identityRepository.GetRoleByNameAsync(sr.Name!)).Select(r => r.Result).Where(r => r != null);
			await identityRepository.AddToRolesAsync(user, roles.Select(r => r!));
		}

		return user;
	}
}
