using Bat.Blazor.App.Services;
using Bat.Shared.Bootstrap;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Bootstrap;

/// <summary>
/// Bootstrapper that registers identity/auth services.
/// </summary>
/// <remarks>
///		Bootstrappers in Blazor.App project are shared between for Blazor Server and Blazor WASM.
/// </remarks>
[Bootstrapper]
public class AuthBootstrapper
{
	public static void ConfigureServices(IServiceCollection services)
	{
		// set up authorization
		services.AddAuthorizationCore();

		/* this has been done in Routes.razor with <CascadingAuthenticationState> tag */
		//services.AddCascadingAuthenticationState();

		// register the custom state provider
		services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

		//// https://stackoverflow.com/questions/70133682/hide-console-authorization-logs-in-blazor-webassembly
		//wasmAppBuilder.Logging.AddFilter("Microsoft.AspNetCore.Authorization.*", LogLevel.Warning);
	}
}
