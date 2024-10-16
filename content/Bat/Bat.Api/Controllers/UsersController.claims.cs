using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

public partial class UsersController
{
	private static readonly ObjectResult ResponseAllClaims = ResponseOk(BuiltinClaims.ALL_CLAIMS.Select(c => new ClaimResp
	{
		ClaimType = c.Type,
		ClaimValue = c.Value
	}).ToList());

	[HttpGet(IApiClient.API_ENDPOINT_CLAIMS)]
	public ActionResult<ApiResp<IEnumerable<ClaimResp>>> GetAllClaims()
	{
		return ResponseAllClaims;
	}
}
