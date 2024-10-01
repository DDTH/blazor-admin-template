using Bat.Blazor.Client.Services;
using Bat.Shared.Bootstrap;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Bat.Blazor.Client.Bootstrap;

/// <summary>
/// Bootstrapper that initializes background routines to sync/keep-alive with backend.
/// </summary>
/// <remarks>
///		Bootstrappers in Blazor.Client project are invoked only in WebAssembly mode.
/// </remarks>
[Bootstrapper]
public class AliveSyncBootstrapper
{
	public static void ConfigureWasmBuilder(WebAssemblyHostBuilder wasmAppBuilder)
	{
		// first, register the services as singletons
		wasmAppBuilder.Services.AddSingleton<InfoSyncService>();
	}

	public static void DecorateWasmApp(WebAssemblyHost app)
	{
		// second, start the services
		app.Services.GetRequiredService<InfoSyncService>();
	}
}
