using Bat.Shared.Bootstrap;
using Bat.Shared.ExternalLoginHelper;

namespace Bat.Api.Bootstrap;

/// <summary>
/// Bootstrapper that setups logging-in with external providers.
/// </summary>
[Bootstrapper]
public class ExternalLoginBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<ExternalLoginBootstrapper>();
		logger.LogInformation("Configuring external login providers...");

		var configuration = appBuilder.Configuration;
		var services = appBuilder.Services;
		services.AddSingleton(sp => ExternalLoginBuilder.New(sp).WithProvidersConfig(configuration).Build());
	}
}
