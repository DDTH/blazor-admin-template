using Bat.Api;
using Bat.Shared.Helpers;

var assemblies = AppDomain.CurrentDomain.GetAssemblies();
var appBuilder = WebApplication.CreateBuilder(args);
var tasks = AppBootstrapper.Bootstrap(out var app, appBuilder, assemblies);
await Task.Run(() =>
{
	var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	Globals.Ready = true; // server is ready to handle requests
	logger.LogInformation("Background bootstrapping completed.");
});
app.Run();
