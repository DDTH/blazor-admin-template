using Bat.Demo.Shared.EF;
using Bat.Demo.Shared.Models;
using Bat.Shared.Api.Helpers;
using Bat.Shared.Bootstrap;

namespace Bat.Demo.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that initializes DbContext/DbContextPool services.
/// </summary>
[Bootstrapper]
public class DbContextBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DbContextBootstrapper>();

		const string confKey = "Databases:Application";
		logger.LogInformation("Configuring DbContext service {confKey}...", confKey);
		DbBootstrapHelper.ConfigureDbContext<IApplicationRepository, ApplicationDbContextRepository>(appBuilder, confKey, logger);
		appBuilder.Services.AddHostedService<ApplicationInitializer>();
	}
}
