using Bat.Shared.Api;
using Bat.Shared.Identity;
using Bat.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_APPLICATION_MANAGER)]
public partial class AppsController : ApiBaseController
{
	[HttpGet(IApiClient.API_ENDPOINT_APPS)]
	public async Task<ActionResult<ApiResp<AppResp>>> GetAllApps(IApplicationRepository applicationRepository)
	{
		var apps = applicationRepository.GetAllAsync();
		var result = new List<AppResp>();
		await foreach (var app in apps)
		{
			result.Add(AppResp.BuildFromApp(app));
		}
		return ResponseOk(apps);
	}
}
