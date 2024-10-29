using Bat.Blazor.App.Helpers;
using Bat.Shared.Api;
using Bat.Shared.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Bat.Blazor.App.Bootstrap;

/// <summary>
/// Bootstrapper that initializes background routines to sync/keep-alive with backend.
/// </summary>
/// <remarks>
///		Bootstrappers in Blazor.App project are shared between for Blazor Server and Blazor WASM.
/// </remarks>
[Bootstrapper]
public class AliveSyncBootstrapper
{

	/* TODO/WORKAROUND: IHostedService does not work with WASM yet. */

	public static void ConfigureServices(IServiceCollection services)
	{
		// first, register the services as singletons
		services.AddSingleton<BackgroundTimerService, InfoSyncService>();
		services.AddSingleton<BackgroundTimerService, AuthTokenRefresherService>();
	}

	public static void InitializeServices(IServiceProvider services)
	{
		// second, start the services
		services.GetServices<BackgroundTimerService>();
	}
}

/*----------------------------------------------------------------------*/

public abstract class BackgroundTimerService : IDisposable
{
	public enum BlazorEnvironment
	{
		All,
		BlazorServerOnly,
		BlazorWasmOnly
	}

	protected readonly Timer? timer;

	private readonly ILogger logger;

	public BackgroundTimerService(TimeSpan initialDelay, TimeSpan interval, ILogger logger, BlazorEnvironment environment = BlazorEnvironment.All)
	{
		this.logger = logger;
		var isBrowser = OperatingSystem.IsBrowser();
		switch (environment)
		{
			case BlazorEnvironment.BlazorServerOnly:
				if (isBrowser)
				{
					logger.LogWarning("{service} is not enabled in Blazor WebAssembly mode.", GetType().FullName);
					return;
				}
				break;
			case BlazorEnvironment.BlazorWasmOnly:
				if (!isBrowser)
				{
					logger.LogWarning("{service} is not enabled in Blazor Server mode.", GetType().FullName);
					return;
				}
				break;
		}
		timer = new Timer(DoWorkInternal, null, initialDelay, interval);
	}

	private async void DoWorkInternal(object? state)
	{
		await Task.Run(() =>
		{
			try
			{
				DoWork(state);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error doing work: {message}", ex.Message);
			}
		});
	}

	protected abstract void DoWork(object? state);

	protected virtual void Dispose(bool disposing)
	{
		timer?.Dispose();
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

/*----------------------------------------------------------------------*/

/// <summary>
/// Background service that periodically ping the server for information.
/// </summary>
public sealed class InfoSyncService : BackgroundTimerService
{
	private static readonly TimeSpan initialDelay = TimeSpan.Zero; // TimeSpan.Zero means start immediately
	private static readonly TimeSpan interval = TimeSpan.FromMinutes(60);

	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<InfoSyncService> logger;

	public InfoSyncService(IServiceProvider serviceProvider, ILogger<InfoSyncService> logger)
		: base(initialDelay, interval, logger, BlazorEnvironment.BlazorWasmOnly)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
		logger.LogInformation("{service} initialized.", GetType().FullName);
	}

	protected override async void DoWork(object? state)
	{
		logger.LogDebug("{service} - pinging server...", GetType().FullName);
		using (var scope = serviceProvider.CreateScope())
		{
			var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
			var infoResp = await apiClient.InfoAsync();
			if (infoResp.Status != 200)
			{
				logger.LogError("{service} - error calling API 'info': {result}", GetType().FullName, JsonSerializer.Serialize(infoResp));
			}
			else
			{
				Globals.AppInfo = infoResp.Data?.App;
				Globals.ServerInfo = infoResp.Data?.Server;
				Globals.CryptoInfo = infoResp.Data?.Crypto;
				Globals.Ready = true;
			}
		}
	}
}

/*----------------------------------------------------------------------*/

/// <summary>
/// Background service that automatically refresh the auth token in the background.
/// </summary>
public sealed class AuthTokenRefresherService : BackgroundTimerService
{
	private static readonly TimeSpan initialDelay = TimeSpan.Zero; // TimeSpan.Zero means start immediately
	private static readonly TimeSpan interval = TimeSpan.FromSeconds(15);

	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<AuthTokenRefresherService> logger;

	public AuthTokenRefresherService(IServiceProvider serviceProvider, ILogger<AuthTokenRefresherService> logger)
		: base(initialDelay, interval, logger, BlazorEnvironment.BlazorWasmOnly)
	{
		this.serviceProvider = serviceProvider;
		this.logger = logger;
		logger.LogInformation("{service} initialized.", GetType().FullName);
	}

	protected override async void DoWork(object? state)
	{
		logger.LogDebug("{service} - refreshing auth token...", GetType().FullName);
		using (var scope = serviceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
			logger.LogWarning("{service} - Auth Token: {token}", GetType().FullName, authToken);
		}
	}
}
