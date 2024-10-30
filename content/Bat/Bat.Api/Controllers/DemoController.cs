using Bat.Api.Controllers;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Shared.Api;

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
	public async Task<ActionResult<ApiResp<IEnumerable<UserResp>>>> GetSeedUsers(ILogger<DemoController> logger, IConfiguration appConfig, IIdentityRepository identityRepository)
	{
		var result = new List<UserResp>();

		if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
		{
			var seedUsers = appConfig.GetSection("SeedingData:Identity:Users").Get<IEnumerable<SeedingUser>>() ?? [];
			foreach (var u in seedUsers)
			{
				var email = u.Email?.ToLower().Trim() ?? string.Empty;
				var user = await identityRepository.GetUserByEmailAsync(email);
				if (user != null)
				{
					var password = Environment.GetEnvironmentVariable($"USER_SECRET_I_{user.Id}") ?? string.Empty;
					logger.LogCritical("DO NOT USE THIS IN PRODUCTION! Returning user '{id}': {secret}", $"USER_SECRET_I_{user.Id}", password);
					result.Add(new UserResp
					{
						Id = user.Id,
						Username = user.UserName!,
						Email = user.Email!,
						Password = password,
					});
				}
			}
		}
		return ResponseOk(result);
	}
}
