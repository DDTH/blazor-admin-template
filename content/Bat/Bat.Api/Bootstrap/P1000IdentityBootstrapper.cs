using Bat.Shared.Bootstrap;
using Bat.Shared.EF.Identity;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Identity;

namespace Bat.Api.Bootstrap;

sealed class Defaults
{
	public static readonly PasswordOptions passwordOptions = new()
	{
		RequiredLength = 12,
		RequiredUniqueChars = 5,
		RequireDigit = true,
		RequireLowercase = true,
		RequireUppercase = true,
		RequireNonAlphanumeric = false,
	};

	public static readonly ClaimsIdentityOptions claimsIdentityOptions = new()
	{
		EmailClaimType = "ema",
		RoleClaimType = "rol",
		UserIdClaimType = "uid",
		UserNameClaimType = "una",
		SecurityStampClaimType = "sec",
	};
}

/// <summary>
/// Built-in bootstrapper that initializes Asp.Net Core Identity services.
/// </summary>
[Bootstrapper]
public class IdentityBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<IdentityBootstrapper>();
		logger.LogInformation("Configuring Identity services...");

		// https://github.com/dotnet/aspnetcore/issues/26119
		// Use .AddIdentityCore<User> then add necessary services manually (e.g. AddRoles, AddSignInManager, etc.)
		// instead of using .AddIdentity<User, Role>
		appBuilder.Services
			.AddIdentityCore<BatUser>(opts =>
			{
				opts.Password = Defaults.passwordOptions;
				opts.ClaimsIdentity = Defaults.claimsIdentityOptions;
			})
			//.AddRoles<BatRole>()
			//.AddSignInManager<SignInManager<BatUser>>()
			.AddEntityFrameworkStores<IdentityDbContextRepository>()
			;
	}
}
