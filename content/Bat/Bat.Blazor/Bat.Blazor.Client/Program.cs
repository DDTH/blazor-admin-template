using Bat.Blazor.Client;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text.Json;

var wasmAppBuilder = WebAssemblyHostBuilder.CreateDefault(args);
Globals.ApiBaseUrl = wasmAppBuilder.HostEnvironment.BaseAddress.TrimEnd('/');
var tasks = WasmAppBootstrapper.Bootstrap(wasmAppBuilder, out var app);

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
await Task.Run(() =>
{
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	Globals.Ready = true; // application is ready
	logger.LogInformation("Background bootstrapping completed.");
});

await app.RunAsync();
