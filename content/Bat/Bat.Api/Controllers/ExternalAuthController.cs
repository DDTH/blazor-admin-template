using Bat.Shared.Api;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

public class ExternalAuthController : ApiBaseController
{
	/// <summary>
	/// Gets the list of external authentication providers.
	/// </summary>
	[HttpGet("/api/external-auth/providers")]
	public ActionResult<ApiResp<IEnumerable<string>>> GetExternalAuthProviders(IConfiguration appConfig)
	{
		var authConfig = new Dictionary<string, object>();
		appConfig.GetSection("Authentication").Bind(authConfig);
		var providers = authConfig.Keys.OrderBy(k => k).ToList();
		return ResponseOk(providers);
	}
}
