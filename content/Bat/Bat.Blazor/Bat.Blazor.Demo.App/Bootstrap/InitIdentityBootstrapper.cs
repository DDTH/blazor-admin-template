using Bat.Demo.Shared.Identity;
using Bat.Shared.Bootstrap;
using Bat.Shared.Global;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bat.Blazor.Demo.App.Bootstrap;

[Bootstrapper(Priority = 100)]
public class InitIdentityBootstrapper
{
	public static void ConfigureServices(IServiceCollection _, ILogger<InitIdentityBootstrapper> logger)
	{
		logger.LogInformation("Registering demo {count} claims...", DemoClaims.ALL_CLAIMS.Count());
		GlobalRegistry.ALL_CLAIMS.UnionWith(DemoClaims.ALL_CLAIMS);
		logger.LogInformation("Total registered claims: {count}", GlobalRegistry.ALL_CLAIMS.Count);
	}
}
