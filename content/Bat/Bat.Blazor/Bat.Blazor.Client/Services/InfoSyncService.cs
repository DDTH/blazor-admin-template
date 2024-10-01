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

	public InfoSyncService(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
		logger = serviceProvider.GetRequiredService<ILogger<InfoSyncService>>();
		timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
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
		}
	}
}
