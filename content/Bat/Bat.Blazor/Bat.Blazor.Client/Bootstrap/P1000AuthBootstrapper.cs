using Bat.Blazor.App.Services;
using Bat.Shared.Bootstrap;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Bat.Blazor.Client.Bootstrap;

/// <summary>
/// Bootstrapper that registers identity/auth services.
/// </summary>
/// <remarks>
///		Bootstrappers in Blazor.Client project are invoked only in WebAssembly mode.
/// </remarks>
[Bootstrapper]
public class AuthBootstrapper
{
	public static void ConfigureWasmBuilder(WebAssemblyHostBuilder wasmAppBuilder)
	{
		// set up authorization
		wasmAppBuilder.Services.AddAuthorizationCore();

		// register the custom state provider
		wasmAppBuilder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

		// https://stackoverflow.com/questions/70133682/hide-console-authorization-logs-in-blazor-webassembly
		wasmAppBuilder.Logging.AddFilter("Microsoft.AspNetCore.Authorization.*", LogLevel.Warning);
	}
}
