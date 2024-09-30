using Bat.Shared.Bootstrap;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Bat.Blazor.Client.Bootstrap;

[Bootstrapper]
public class HttpClientBootstrapper
{
	public static void ConfigureWasmBuilder(WebAssemblyHostBuilder wasmAppBuilder)
	{
		wasmAppBuilder.Services.AddHttpClient();
		//wasmAppBuilder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(wasmAppBuilder.HostEnvironment.BaseAddress) });
	}
}
