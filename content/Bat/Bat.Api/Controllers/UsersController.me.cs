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
	/// <returns></returns>
	[HttpGet("/api/users/-me")]
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResp>>> GetMyInfo(
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository)
	{
		var currentUser = await GetCurrentUserAsync(identityOptions.Value, identityRepository);
		if (currentUser == null)
		{
			return _respAuthenticationRequired;
		}
		return ResponseOk(UserResp.BuildFromUser(currentUser));
	}

	/// <summary>
	/// Updates current user's profile.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <returns></returns>
	[HttpPost("/api/users/-me/profile")]
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResp>>> UpdateMyProfile(
		[FromBody] UpdateUserProfileReq req,
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository)
	{
		var currentUser = await GetCurrentUserAsync(identityOptions.Value, identityRepository);
		if (currentUser == null)
		{
			return _respAuthenticationRequired;
		}
		currentUser.GivenName = req.GivenName ?? currentUser.GivenName;
		currentUser.FamilyName = req.FamilyName ?? currentUser.FamilyName;
		var user = await identityRepository.UpdateAsync(currentUser);
		if (user == null)
		{
			return ResponseNoData(500, "Failed to update user profile.");
		}
		return ResponseOk(UserResp.BuildFromUser(user));
	}
}
