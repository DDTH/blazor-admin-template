using Bat.Shared.Bootstrap;
using Bat.Shared.Global;
using Bat.Shared.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bat.Blazor.App.Bootstrap;

/// <summary>
/// Bootstrapper that register identity data.
/// </summary>
/// <remarks>
///		Bootstrappers in Blazor.App project are shared between for Blazor Server and Blazor WASM.
/// </remarks>
[Bootstrapper(Priority = 100)]
public class InitIdentityBootstrapper
{
	public static void ConfigureServices(IServiceCollection _, ILogger<InitIdentityBootstrapper> logger)
	{
		logger.LogInformation("Registering built-in {count} claims...", BuiltinClaims.ALL_CLAIMS.Count());
		GlobalRegistry.ALL_CLAIMS.UnionWith(BuiltinClaims.ALL_CLAIMS);
		logger.LogInformation("Total registered claims: {count}", GlobalRegistry.ALL_CLAIMS.Count);
	}
}
