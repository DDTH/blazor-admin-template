using Bat.Blazor.Client;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var wasmAppBuilder = WebAssemblyHostBuilder.CreateDefault(args);
Bat.Blazor.App.Globals.ApiBaseUrl = wasmAppBuilder.HostEnvironment.BaseAddress;
var tasks = WasmAppBootstrapper.Bootstrap(wasmAppBuilder, out var app);

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
await Task.Run(() =>
{
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	logger.LogInformation("Background bootstrapping completed.");
});

await app.RunAsync();
