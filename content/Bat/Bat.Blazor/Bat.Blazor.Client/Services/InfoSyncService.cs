using Bat.Shared.Api;
using System.Text.Json;

namespace Bat.Blazor.Client.Services;

/// <summary>
/// Background service that periodically ping the server for information.
/// </summary>
public sealed class InfoSyncService : IDisposable
{
	private readonly IServiceProvider serviceProvider;
	private readonly ILogger<InfoSyncService> logger;
	private readonly Timer timer;

	private readonly TimeSpan initialDelay = TimeSpan.Zero; // TimeSpan.Zero means start immediately
	private readonly TimeSpan interval = TimeSpan.FromMinutes(15);

	public InfoSyncService(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
		logger = serviceProvider.GetRequiredService<ILogger<InfoSyncService>>();
		timer = new Timer(DoWork, null, initialDelay, interval);
		logger.LogInformation("{service} initialized.", nameof(InfoSyncService));
	}

	public void Dispose()
	{
		timer?.Dispose();
	}

	private async void DoWork(object? state)
	{
		var apiClient = serviceProvider.GetRequiredService<IApiClient>();
		var infoResp = await apiClient.InfoAsync();
		if (infoResp.Status != 200)
		{
			logger.LogError("{service} - error calling API 'info': {result}", nameof(InfoSyncService), JsonSerializer.Serialize(infoResp));
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
