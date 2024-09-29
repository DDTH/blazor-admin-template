using Bat.Blazor.Client;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var wasmAppBuilder = WebAssemblyHostBuilder.CreateDefault(args);
var tasks = WasmAppBootstrapper.Bootstrap(wasmAppBuilder, out var app);
await Task.Run(() =>
{
	Console.WriteLine("[INFO] Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks);
	Globals.Ready = true; // server is ready to handle requests
	Console.WriteLine("[INFO] Background bootstrapping completed.");
});
await app.RunAsync();
