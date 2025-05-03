using System.Reflection;
using Bat.Blazor.Client;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var wasmAppBuilder = WebAssemblyHostBuilder.CreateDefault(args);

// load custom assemblies for bootstrapping
var assemblies = AppDomain.CurrentDomain.GetAssemblies();
var additionalAssemblies = wasmAppBuilder.Configuration.GetSection("Bootstrap:Assemblies").Get<List<string>>() ?? [];
Console.WriteLine($"[INFO] Loading additional assemblies...");
foreach (var assemblyName in additionalAssemblies)
{
	Console.WriteLine($"[INFO] -- Loading assembly [{assemblyName}]...");
	try
	{
		assemblies = [.. assemblies, Assembly.Load(assemblyName)];
	}
	catch (Exception e) when (e is ArgumentException || e is FileNotFoundException || e is FileLoadException || e is BadImageFormatException)
	{
		Console.WriteLine($"[ERROR] -- Failed to load assembly [{assemblyName}]: {e.Message}");
	}
}

Bat.Blazor.App.Globals.ApiBaseUrl = string.IsNullOrEmpty(wasmAppBuilder.Configuration[Bat.Blazor.App.Globals.CONF_KEY_API_BASE_URL])
	? wasmAppBuilder.HostEnvironment.BaseAddress
	: wasmAppBuilder.Configuration[Bat.Blazor.App.Globals.CONF_KEY_API_BASE_URL];

// Bootstrapping
var tasks = WasmAppBootstrapper.Bootstrap(out var app, wasmAppBuilder, assemblies);
await Task.Run(() =>
{
	var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	logger.LogInformation("Background bootstrapping completed.");
});

await app.RunAsync();
