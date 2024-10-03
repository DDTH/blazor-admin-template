using Bat.Blazor.App.Services;
using Bat.Shared.Api;
using Bat.Shared.Bootstrap;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace Bat.Blazor.Client.Bootstrap;

/// <summary>
/// Bootstrapper that registers client-services used by Blazor client.
/// </summary>
/// <remarks>
///		Bootstrappers for Blazor server and Blazor client should be separated in different projects.
/// </remarks>
[Bootstrapper]
public class ClientBootstrapper
{
	public static void ConfigureWasmBuilder(WebAssemblyHostBuilder wasmAppBuilder)
	{
		wasmAppBuilder.Services.AddHttpClient();
		wasmAppBuilder.Services.AddSingleton<IApiClient, ApiClient>();

		// https://stackoverflow.com/questions/52889827/remove-http-client-logging-handler-in-asp-net-core
		wasmAppBuilder.Services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
	}
}
