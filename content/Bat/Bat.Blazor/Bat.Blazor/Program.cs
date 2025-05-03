using System.Reflection;
using Bat.Shared.Helpers;

var appBuilder = WebApplication.CreateBuilder(args);

// load custom assemblies for bootstrapping
var assemblies = AppDomain.CurrentDomain.GetAssemblies();
var additionalAssemblies = appBuilder.Configuration.GetSection("Bootstrap:Assemblies").Get<List<string>>() ?? [];
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

Bat.Blazor.App.Globals.ApiBaseUrl = string.IsNullOrEmpty(appBuilder.Configuration[Bat.Blazor.App.Globals.CONF_KEY_API_BASE_URL])
	? null
	: appBuilder.Configuration[Bat.Blazor.App.Globals.CONF_KEY_API_BASE_URL];

// Bootstrapping
var tasks = Bat.Api.AppBootstrapper.Bootstrap(out var app, appBuilder, assemblies);
await Task.Run(() =>
{
	var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	Bat.Shared.Api.Globals.Ready = true; // server is ready to handle requests
	logger.LogInformation("Background bootstrapping completed.");
});
app.Run();
