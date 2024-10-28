using System.Security.Cryptography;
using Bat.Api.Services;
using Bat.Shared.Api;
using Bat.Shared.Identity;
using Bat.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_APPLICATION_MANAGER)]
public partial class AppsController : ApiBaseController
{
	/// <summary>
	/// Gets all available applications.
	/// </summary>
	/// <param name="applicationRepository"></param>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_APPS)]
	public async Task<ActionResult<ApiResp<AppResp>>> GetAllApps(IApplicationRepository applicationRepository)
	{
		var apps = applicationRepository.GetAllAsync();
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
	/// <param name="applicationRepository"></param>
	/// <returns></returns>
	///
	[HttpGet(IApiClient.API_ENDPOINT_APPS_ID)]
	public async Task<ActionResult<ApiResp<AppResp>>> GetApp(
		[FromRoute] string id,
		IApplicationRepository applicationRepository)
	{
		var app = await applicationRepository.GetByIDAsync(id);
		if (app == null)
		{
			return ResponseNoData(404, $"Application '{id}' not found.");
		}
		return ResponseOk(AppResp.BuildFromApp(app));
	}

	/// <summary>
	/// Creates a new application.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="applicationRepository"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	[HttpPost(IApiClient.API_ENDPOINT_APPS)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_APP_PERM)]
	public async Task<ActionResult<ApiResp<AppResp>>> CreateApp(
		[FromBody] CreateOrUpdateAppReq req,
		IApplicationRepository applicationRepository,
		IAuthenticator? authenticator,
		IAuthenticatorAsync? authenticatorAsync)
	{
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.");
		}

		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(authenticator, authenticatorAsync, jwtToken!);
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
				var rsa = new RSACryptoServiceProvider();
				rsa.ImportFromPem(req.PublicKeyPEM);
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
		app = await applicationRepository.CreateAsync(app);
		return ResponseOk(AppResp.BuildFromApp(app));
	}

	/// <summary>
	/// Updates an existing application.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="req"></param>
	/// <param name="applicationRepository"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	[HttpPut(IApiClient.API_ENDPOINT_APPS_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM)]
	public async Task<ActionResult<ApiResp<AppResp>>> UpdateApp(
		[FromRoute] string id,
		[FromBody] CreateOrUpdateAppReq req,
		IApplicationRepository applicationRepository,
		IAuthenticator? authenticator,
		IAuthenticatorAsync? authenticatorAsync)
	{
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.");
		}

		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(authenticator, authenticatorAsync, jwtToken!);
		if (tokenValidationResult.Status != 200)
		{
			// the auth token should still be valid
			return ResponseNoData(403, tokenValidationResult.Error);
		}

		var app = await applicationRepository.GetByIDAsync(id);
		if (app == null)
		{
			return ResponseNoData(404, $"Application '{id}' not found.");
		}

		if (!string.IsNullOrWhiteSpace(req.PublicKeyPEM))
		{
			// Validate public key PEM
			try
			{
				var rsa = new RSACryptoServiceProvider();
				rsa.ImportFromPem(req.PublicKeyPEM);
			}
			catch (Exception ex) when (ex is CryptographicException || ex is ArgumentException)
			{
				return ResponseNoData(400, $"Invalid RSA public key: {ex.Message}");
			}
		}

		app.DisplayName = req.DisplayName?.Trim() ?? app.DisplayName; // Update display name if provided
		app.PublicKeyPEM = req.PublicKeyPEM?.Trim() ?? app.PublicKeyPEM; // Update public key if provided

		app = await applicationRepository.UpdateAsync(app);
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
	/// <param name="applicationRepository"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	[HttpDelete(IApiClient.API_ENDPOINT_APPS_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM)]
	public async Task<ActionResult<ApiResp<AppResp>>> DeleteApp(
		[FromRoute] string id,
		IApplicationRepository applicationRepository,
		IAuthenticator? authenticator,
		IAuthenticatorAsync? authenticatorAsync)
	{
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.");
		}

		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(authenticator, authenticatorAsync, jwtToken!);
		if (tokenValidationResult.Status != 200)
		{
			// the auth token should still be valid
			return ResponseNoData(403, tokenValidationResult.Error);
		}

		var app = await applicationRepository.GetByIDAsync(id);
		if (app == null)
		{
			return ResponseNoData(404, $"Application '{id}' not found.");
		}

		var resultDelete = await applicationRepository.DeleteAsync(app);
		if (!resultDelete)
		{
			return ResponseNoData(500, $"Failed to delete application '{id}'.");
		}
		return ResponseOk(AppResp.BuildFromApp(app));
	}
}
