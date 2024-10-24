using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

public partial class UsersController
{
	private static readonly ObjectResult ResponseAllClaims = ResponseOk(BuiltinClaims.ALL_CLAIMS.Select(c => new ClaimResp
	{
		ClaimType = c.Type,
		ClaimValue = c.Value
	}).ToList());

	/// <summary>
	/// Gets all available claims.
	/// </summary>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_CLAIMS)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public ActionResult<ApiResp<IEnumerable<ClaimResp>>> GetAllClaims()
	{
		return ResponseAllClaims;
	}
}
