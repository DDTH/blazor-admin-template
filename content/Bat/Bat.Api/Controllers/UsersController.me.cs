using Bat.Api.Services;
using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bat.Api.Controllers;

public partial class UsersController
{
	/// <summary>
	/// Returns current user's information.
	/// </summary>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	[HttpGet("/api/users/-me")]
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResp>>> GetMyInfo(
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync)
	{
		var (vAuthTokenResult, currentUser) = await VerifyAuthTokenAndCurrentUser(
			identityRepository,
			identityOptions.Value,
			authenticator, authenticatorAsync);
		if (vAuthTokenResult != null)
		{
			// current auth token and signed-in user should all be valid
			return vAuthTokenResult;
		}
		return ResponseOk(UserResp.BuildFromUser(currentUser));
	}

	/// <summary>
	/// Updates current user's profile.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="lookupNormalizer"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	[HttpPost("/api/users/-me/profile")]
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResp>>> UpdateMyProfile(
		[FromBody] UpdateUserProfileReq req,
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository,
		ILookupNormalizer lookupNormalizer,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync)
	{
		var (vAuthTokenResult, currentUser) = await VerifyAuthTokenAndCurrentUser(
			identityRepository,
			identityOptions.Value,
			authenticator, authenticatorAsync);
		if (vAuthTokenResult != null)
		{
			// current auth token and signed-in user should all be valid
			return vAuthTokenResult;
		}
		if (!string.IsNullOrWhiteSpace(req.Email))
		{
			var existingUser = await identityRepository.GetUserByEmailAsync(req.Email);
			if (existingUser != null && existingUser.Id != currentUser.Id)
			{
				return ResponseNoData(400, "Email is being used by another user.");
			}
			currentUser.Email = req.Email.ToLower().Trim();
			currentUser.NormalizedEmail = lookupNormalizer.NormalizeEmail(currentUser.Email);
		}
		currentUser.GivenName = (req.GivenName ?? currentUser.GivenName)?.Trim();
		currentUser.FamilyName = (req.FamilyName ?? currentUser.FamilyName)?.Trim();
		var user = await identityRepository.UpdateAsync(currentUser);
		if (user == null)
		{
			return ResponseNoData(500, "Failed to update user profile.");
		}
		return ResponseOk(UserResp.BuildFromUser(user));
	}

	/// <summary>
	/// Changes current user's password.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="passwordValidator"></param>
	/// <param name="passwordHasher"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <param name="userManager"></param>
	/// <returns></returns>
	[HttpPost("/api/users/-me/password")]
	[Authorize]
	public async Task<ActionResult<ApiResp<ChangePasswordResp>>> ChangeMyPassword(
		[FromBody] ChangePasswordReq req,
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository,
		IPasswordValidator<BatUser> passwordValidator,
		IPasswordHasher<BatUser> passwordHasher,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync,
		UserManager<BatUser> userManager)
	{
		var (vAuthTokenResult, currentUser) = await VerifyAuthTokenAndCurrentUser(
			identityRepository,
			identityOptions.Value,
			authenticator, authenticatorAsync);
		if (vAuthTokenResult != null)
		{
			// current auth token and signed-in user should all be valid
			return vAuthTokenResult;
		}

		// verify current password
		if (passwordHasher.VerifyHashedPassword(currentUser, currentUser.PasswordHash!, req.CurrentPassword) == PasswordVerificationResult.Failed)
		{
			return ResponseNoData(403, "Invalid user/password combination.");
		}

		// verify if the new password meets the compexity requirements
		var vresult = await passwordValidator.ValidateAsync(userManager, null!, req.NewPassword);
		if (vresult != IdentityResult.Success)
		{
			return ResponseNoData(400, vresult.ToString());
		}

		// change password
		currentUser.PasswordHash = passwordHasher.HashPassword(currentUser, req.NewPassword);
		currentUser = await identityRepository.UpdateAsync(currentUser);
		if (currentUser == null)
		{
			return ResponseNoData(500, "Failed to update user.");
		}

		// if password changed successfully, invalidate all previous auth tokens by changing the security stamp
		currentUser = await identityRepository.UpdateSecurityStampAsync(currentUser);
		if (currentUser == null)
		{
			return ResponseNoData(500, "Failed to update user.");
		}

		var jwtToken = GetAuthToken();

		var refreshResult = authenticatorAsync != null
			? await authenticatorAsync.RefreshAsync(jwtToken!, true)
			: authenticator!.Refresh(jwtToken!, true);

		return ResponseOk("Password changed successfully.", new ChangePasswordResp
		{
			Token = refreshResult!.Token!, // changing password should invalidate all previous auth tokens
		});
	}
}
