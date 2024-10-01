using Bat.Blazor.Client.Services;
using Bat.Shared.Api;
using Bat.Shared.Bootstrap;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Bat.Blazor.Client.Bootstrap;

/// <summary>
/// Bootstrapper that registers services used by Blazor client.
/// </summary>
/// <remarks>
///		Bootstrappers in Blazor.Client project are invoked only in WebAssembly mode.
/// </remarks>
[Bootstrapper]
public class ClientBootstrapper
{
	public static void ConfigureWasmBuilder(WebAssemblyHostBuilder wasmAppBuilder)
	{
		wasmAppBuilder.Services.AddHttpClient();
		wasmAppBuilder.Services.AddSingleton<IApiClient, HttpApiClient>();
	}
}
