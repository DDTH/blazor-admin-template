using Bat.Shared.Api.Helpers;
using Bat.Shared.Bootstrap;
using Bat.Shared.EF.Identity;
using Bat.Shared.Identity;

namespace Bat.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that initializes DbContext/DbContextPool services.
/// </summary>
[Bootstrapper]
public class DbContextBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DbContextBootstrapper>();

		const string confKey = "Databases:Identity";
		logger.LogInformation("Configuring DbContext service {confKey}...", confKey);
		DbBootstrapHelper.ConfigureDbContext<IIdentityRepository, IdentityDbContextRepository>(appBuilder, confKey, logger);
		appBuilder.Services.AddHostedService<IdentityInitializer>();
	}
}
