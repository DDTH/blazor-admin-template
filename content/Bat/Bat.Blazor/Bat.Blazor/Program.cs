using Bat.Shared.Helpers;

var assemblies = AppDomain.CurrentDomain.GetAssemblies()
	.Append(typeof(Bat.Api.Globals).Assembly) // Bat.Api is used in Blazor Server, add its assembly to the list
	.Append(typeof(Bat.Blazor.App.Globals).Assembly); // Bat.Blazor.App is shared between Blazor Server and WebAssembly, add its assembly to the list
var appBuilder = WebApplication.CreateBuilder(args);
Bat.Blazor.App.Globals.ApiBaseUrl = string.IsNullOrEmpty(appBuilder.Configuration[Bat.Blazor.App.Globals.CONF_KEY_API_BASE_URL])
	? null
	: appBuilder.Configuration[Bat.Blazor.App.Globals.CONF_KEY_API_BASE_URL];
var tasks = Bat.Api.AppBootstrapper.Bootstrap(out var app, appBuilder, assemblies);
await Task.Run(() =>
{
	var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	Bat.Api.Globals.Ready = true; // server is ready to handle requests
	logger.LogInformation("Background bootstrapping completed.");
});
app.Run();
