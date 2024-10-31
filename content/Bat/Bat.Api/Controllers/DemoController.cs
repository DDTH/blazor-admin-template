using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

/// <summary>
/// For demonstration purposes only!
/// </summary>
public class DemoController : ApiBaseController
{
	struct SeedingUser
	{
		public string? Id { get; set; }
		public string? UserName { get; set; }
		public string? Email { get; set; }
	}

	/// <summary>
	/// (FOR DEMO PURPOSES ONLY!) Returns all seed users, with their usernames, emails and passwords!
	/// </summary>
	/// <returns></returns>
	[HttpGet("/api/demo/seed_users")]
	public ActionResult<ApiResp<IEnumerable<UserResp>>> GetSeedUsers(IConfiguration appConfig, IIdentityRepository identityRepository)
	{
		var result = new List<UserResp>();

		if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
		{
			var seedUsers = appConfig.GetSection("SeedingData:Identity:Users").Get<IEnumerable<SeedingUser>>() ?? [];
			result.AddRange(seedUsers.Select(su =>
			{
				var email = su.Email?.ToLower().Trim() ?? string.Empty;
				return identityRepository.GetUserByEmailAsync(email).Result;
			}).Where(u => u != null).Select(u => new UserResp
			{
				Id = u!.Id,
				Username = u!.UserName!,
				Email = u!.Email!,
				Password = Environment.GetEnvironmentVariable($"USER_SECRET_I_{u.Id}") ?? string.Empty,
			}));
		}

		return ResponseOk(result);
	}
}
