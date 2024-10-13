using Bat.Shared.EF;
using Bat.Shared.Models;

namespace Bat.Api.Bootstrap;

sealed class ApplicationInitializer(
	IServiceProvider serviceProvider,
	ILogger<ApplicationInitializer> logger,
	IWebHostEnvironment environment) : IHostedService
{
	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Initializing application data...");

		using (var scope = serviceProvider.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationRepository>() as ApplicationDbContextRepository
				?? throw new InvalidOperationException($"Application repository is not an instance of {nameof(ApplicationDbContextRepository)}.");
			var tryParseInitDb = bool.TryParse(Environment.GetEnvironmentVariable(Globals.ENV_INIT_DB), out var initDb);
			if (environment.IsDevelopment() || (tryParseInitDb && initDb))
			{
				logger.LogInformation("Ensuring database schema exist...");
				dbContext.Database.EnsureCreated();
			}
		}

		return Task.CompletedTask;
	}
}
