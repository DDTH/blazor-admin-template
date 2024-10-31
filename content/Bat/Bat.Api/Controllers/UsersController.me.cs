using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

public partial class UsersController
{
	/// <summary>
	/// Returns current user's information.
	/// </summary>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_USERS_ME)]
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResp>>> GetMyInfo()
	{
		var (authErrorResult, currentUser) = await VerifyAuthTokenAndCurrentUser();
		return authErrorResult ?? ResponseOk(UserResp.BuildFromUser(currentUser));
	}

	/// <summary>
	/// Updates current user's profile.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="lookupNormalizer"></param>
	/// <returns></returns>
	[HttpPost(IApiClient.API_ENDPOINT_USERS_ME_PROFILE)]
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResp>>> UpdateMyProfile([FromBody] UpdateUserProfileReq req, ILookupNormalizer lookupNormalizer)
	{
		var (authErrorResult, currentUser) = await VerifyAuthTokenAndCurrentUser();
		if (authErrorResult != null)
		{
			// current auth token and signed-in user should all be valid
			return authErrorResult;
		}
		if (!string.IsNullOrWhiteSpace(req.Email))
		{
			var existingUser = await IdentityRepository.GetUserByEmailAsync(req.Email);
			if (existingUser != null && existingUser.Id != currentUser.Id)
			{
				return ResponseNoData(400, "Email is being used by another user.");
			}
			currentUser.Email = req.Email.ToLower().Trim();
			currentUser.NormalizedEmail = lookupNormalizer.NormalizeEmail(currentUser.Email);
		}
		currentUser.GivenName = (req.GivenName ?? currentUser.GivenName)?.Trim();
		currentUser.FamilyName = (req.FamilyName ?? currentUser.FamilyName)?.Trim();
		var user = await IdentityRepository.UpdateAsync(currentUser);
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
	/// <param name="passwordValidator"></param>
	/// <param name="passwordHasher"></param>
	/// <param name="userManager"></param>
	/// <returns></returns>
	[HttpPost(IApiClient.API_ENDPOINT_USERS_ME_PASSWORD)]
	[Authorize]
	public async Task<ActionResult<ApiResp<ChangePasswordResp>>> ChangeMyPassword(
		[FromBody] ChangePasswordReq req,
		IPasswordValidator<BatUser> passwordValidator,
		IPasswordHasher<BatUser> passwordHasher,
		UserManager<BatUser> userManager)
	{
		var (authErrorResult, currentUser) = await VerifyAuthTokenAndCurrentUser();
		if (authErrorResult != null)
		{
			// current auth token and signed-in user should all be valid
			return authErrorResult;
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
		currentUser = await IdentityRepository.UpdateAsync(currentUser);
		if (currentUser == null)
		{
			return ResponseNoData(500, "Failed to update user.");
		}

		// if password changed successfully, invalidate all previous auth tokens by changing the security stamp
		currentUser = await IdentityRepository.UpdateSecurityStampAsync(currentUser);
		if (currentUser == null)
		{
			return ResponseNoData(500, "Failed to update user.");
		}

		var jwtToken = GetAuthToken();

		var refreshResult = AuthenticatorAsync != null
			? await AuthenticatorAsync.RefreshAsync(jwtToken!, true)
			: Authenticator!.Refresh(jwtToken!, true);

		return ResponseOk("Password changed successfully.", new ChangePasswordResp
		{
			Token = refreshResult!.Token!, // changing password should invalidate all previous auth tokens
		});
	}
}
