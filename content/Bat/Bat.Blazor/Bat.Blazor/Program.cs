using Bat.Api;
using Bat.Shared.Helpers;

var appBuilder = WebApplication.CreateBuilder(args);
var tasks = AppBootstrapper.Bootstrap(appBuilder, out var app);
await Task.Run(() =>
{
	var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Program");
	logger.LogInformation("Waiting for background bootstrapping tasks...");
	AsyncHelper.WaitForBackgroundTasks(tasks, logger);
	Globals.Ready = true; // server is ready to handle requests
	logger.LogInformation("Background bootstrapping completed.");
});
app.Run();
