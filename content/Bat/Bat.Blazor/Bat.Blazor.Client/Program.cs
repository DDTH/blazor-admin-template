using Bat.Blazor.Client;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var wasmAppBuilder = WebAssemblyHostBuilder.CreateDefault(args);
Globals.ApiBaseUrl = wasmAppBuilder.HostEnvironment.BaseAddress;
Console.WriteLine($"API base URL: {Globals.ApiBaseUrl}");
var tasks = WasmAppBootstrapper.Bootstrap(wasmAppBuilder, out var app);

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
await Task.Run(() =>
{
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	Globals.Ready = true; // application is ready
	logger.LogInformation("Background bootstrapping completed.");
});

var httpClient = app.Services.GetRequiredService<HttpClient>();
httpClient.BaseAddress = new Uri(Globals.ApiBaseUrl);
logger.LogInformation("Base address set to {addr}", httpClient.BaseAddress);

await app.RunAsync();
