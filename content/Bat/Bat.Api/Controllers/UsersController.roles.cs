using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

public partial class UsersController
{
	[HttpGet(IApiClient.API_ENDPOINT_ROLES)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public async Task<ActionResult<ApiResp<IEnumerable<RoleResp>>>> GetAllRoles(IIdentityRepository identityRepository)
	{
		var roles = identityRepository.AllRolesAsync();
		var result = new List<RoleResp>();
		await foreach (var role in roles)
		{
			result.Add(RoleResp.BuildFromRole(role));
		}
		return ResponseOk(result);
	}
}
