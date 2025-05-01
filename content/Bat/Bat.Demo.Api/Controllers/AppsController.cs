using System.Security.Cryptography;
using Bat.Demo.Api.Api;
using Bat.Demo.Shared.Models;
using Bat.Shared.Api;
using Bat.Shared.Api.Controller;
using Bat.Shared.Api.Services;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Demo.Api.Controllers;

[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_APPLICATION_MANAGER)]
public partial class AppsController : ApiBaseController
{
	private readonly IAuthenticator? Authenticator;
	private readonly IAuthenticatorAsync? AuthenticatorAsync;
	private readonly IApplicationRepository ApplicationRepository;

	public AppsController(
		IApplicationRepository applicationRepository,
		IAuthenticator? authenticator,
		IAuthenticatorAsync? authenticatorAsync)
	{
		ArgumentNullException.ThrowIfNull(applicationRepository, nameof(applicationRepository));
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined defined.");
		}

		ApplicationRepository = applicationRepository;
		Authenticator = authenticator;
		AuthenticatorAsync = authenticatorAsync;
	}

	/// <summary>
	/// Gets all available applications.
	/// </summary>
	/// <returns></returns>
	[HttpGet(IDemoApiClient.API_ENDPOINT_APPS)]
	public async Task<ActionResult<ApiResp<AppResp>>> GetAllApps()
	{
		var apps = ApplicationRepository.GetAllAsync();
		var result = new List<AppResp>();
		await foreach (var app in apps)
		{
			result.Add(AppResp.BuildFromApp(app));
		}
		return ResponseOk(result);
	}

	/// <summary>
	/// Gets an application by its ID.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	///
	[HttpGet(IDemoApiClient.API_ENDPOINT_APPS_ID)]
	public async Task<ActionResult<ApiResp<AppResp>>> GetApp([FromRoute] string id)
	{
		var app = await ApplicationRepository.GetByIDAsync(id);
		return app == null
			? ResponseNoData(404, $"Application '{id}' not found.")
			: ResponseOk(AppResp.BuildFromApp(app));
	}

	/// <summary>
	/// Creates a new application.
	/// </summary>
	/// <param name="req"></param>
	/// <returns></returns>
	[HttpPost(IDemoApiClient.API_ENDPOINT_APPS)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_APP_PERM)]
	public async Task<ActionResult<ApiResp<AppResp>>> CreateApp([FromBody] CreateOrUpdateAppReq req)
	{
		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(Authenticator, AuthenticatorAsync, jwtToken);
		if (tokenValidationResult.Status != 200)
		{
			// the auth token should still be valid
			return ResponseNoData(403, tokenValidationResult.Error);
		}

		// Validate display name
		if (string.IsNullOrWhiteSpace(req.DisplayName))
		{
			return ResponseNoData(400, "Display name is required.");
		}

		if (!string.IsNullOrWhiteSpace(req.PublicKeyPEM))
		{
			// Validate public key PEM
			try
			{
				using (var rsa = new RSACryptoServiceProvider())
				{
					rsa.ImportFromPem(req.PublicKeyPEM);
				}
			}
			catch (Exception ex) when (ex is CryptographicException || ex is ArgumentException)
			{
				return ResponseNoData(400, $"Invalid RSA public key: {ex.Message}");
			}
		}

		var app = new Application
		{
			DisplayName = req.DisplayName.Trim(),
			PublicKeyPEM = req.PublicKeyPEM?.Trim()
		};
		app = await ApplicationRepository.CreateAsync(app);
		return ResponseOk(AppResp.BuildFromApp(app));
	}

	/// <summary>
	/// Updates an existing application.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="req"></param>
	/// <returns></returns>
	[HttpPut(IDemoApiClient.API_ENDPOINT_APPS_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM)]
	public async Task<ActionResult<ApiResp<AppResp>>> UpdateApp([FromRoute] string id, [FromBody] CreateOrUpdateAppReq req)
	{
		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(Authenticator, AuthenticatorAsync, jwtToken);
		if (tokenValidationResult.Status != 200)
		{
			// the auth token should still be valid
			return ResponseNoData(403, tokenValidationResult.Error);
		}

		var app = await ApplicationRepository.GetByIDAsync(id);
		if (app == null)
		{
			return ResponseNoData(404, $"Application '{id}' not found.");
		}

		if (!string.IsNullOrWhiteSpace(req.PublicKeyPEM))
		{
			// Validate public key PEM
			try
			{
				using (var rsa = new RSACryptoServiceProvider())
				{
					rsa.ImportFromPem(req.PublicKeyPEM);
				}
			}
			catch (Exception ex) when (ex is CryptographicException || ex is ArgumentException)
			{
				return ResponseNoData(400, $"Invalid RSA public key: {ex.Message}");
			}
		}

		app.DisplayName = req.DisplayName?.Trim() ?? app.DisplayName; // Update display name if provided
		app.PublicKeyPEM = req.PublicKeyPEM?.Trim() ?? app.PublicKeyPEM; // Update public key if provided

		app = await ApplicationRepository.UpdateAsync(app);
		if (app == null)
		{
			return ResponseNoData(500, $"Failed to update application '{id}'.");
		}
		return ResponseOk(AppResp.BuildFromApp(app));
	}

	/// <summary>
	/// Deletes an existing application.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[HttpDelete(IDemoApiClient.API_ENDPOINT_APPS_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM)]
	public async Task<ActionResult<ApiResp<AppResp>>> DeleteApp([FromRoute] string id)
	{
		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(Authenticator, AuthenticatorAsync, jwtToken);
		if (tokenValidationResult.Status != 200)
		{
			// the auth token should still be valid
			return ResponseNoData(403, tokenValidationResult.Error);
		}

		var app = await ApplicationRepository.GetByIDAsync(id);
		if (app == null)
		{
			return ResponseNoData(404, $"Application '{id}' not found.");
		}

		var resultDelete = await ApplicationRepository.DeleteAsync(app);
		if (!resultDelete)
		{
			return ResponseNoData(500, $"Failed to delete application '{id}'.");
		}
		return ResponseOk(AppResp.BuildFromApp(app));
	}
}
