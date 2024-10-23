using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

public partial class UsersController
{
	/// <summary>
	/// Gets all available roles.
	/// </summary>
	/// <param name="identityRepository"></param>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_USERS)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public async Task<ActionResult<ApiResp<IEnumerable<UserResp>>>> GetAllUsers(IIdentityRepository identityRepository)
	{
		var users = identityRepository.AllUsersAsync();
		var result = new List<UserResp>();
		await foreach (var user in users)
		{
			user.Roles ??= await identityRepository.GetRolesAsync(user);
			user.Claims ??= await identityRepository.GetClaimsAsync(user);
			result.Add(UserResp.BuildFromUser(user));
		}
		return ResponseOk(result);
	}

	/// <summary>
	/// Gets a user by id.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="identityRepository"></param>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_USERS_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public async Task<ActionResult<ApiResp<UserResp>>> GetUser([FromRoute] string id, IIdentityRepository identityRepository)
	{
		var user = await identityRepository.GetUserByIDAsync(id, UserFetchOptions.DEFAULT.FetchRoles().FetchClaims());
		if (user == null)
		{
			return ResponseNoData(404, $"User '{id}' not found.");
		}
		user.Roles ??= await identityRepository.GetRolesAsync(user);
		user.Claims ??= await identityRepository.GetClaimsAsync(user);
		return ResponseOk(UserResp.BuildFromUser(user));
	}
}
