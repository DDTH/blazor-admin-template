using Bat.Blazor.Client;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var assemblies = AppDomain.CurrentDomain.GetAssemblies()
	.Append(typeof(Bat.Blazor.App.Globals).Assembly); // Bat.Blazor.App is shared between Blazor Server and WebAssembly, add its assembly to the list
var wasmAppBuilder = WebAssemblyHostBuilder.CreateDefault(args);
Bat.Blazor.App.Globals.ApiBaseUrl = string.IsNullOrEmpty(wasmAppBuilder.Configuration[Bat.Blazor.App.Globals.CONF_KEY_API_BASE_URL])
	? wasmAppBuilder.HostEnvironment.BaseAddress
	: wasmAppBuilder.Configuration[Bat.Blazor.App.Globals.CONF_KEY_API_BASE_URL];
var tasks = WasmAppBootstrapper.Bootstrap(out var app, wasmAppBuilder, assemblies);
await Task.Run(() =>
{
	var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	logger.LogInformation("Background bootstrapping completed.");
});

await app.RunAsync();
