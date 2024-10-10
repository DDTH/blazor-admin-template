using Bat.Blazor.App.Services;
using Bat.Shared.Api;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Layout;

/// <summary>
/// Base layout class that provides common properties and utility methods.
/// </summary>
public abstract class BaseLayout : LayoutComponentBase
{
	[Inject]
	protected virtual IServiceProvider ServiceProvider { get; init; } = default!;

	[Inject]
	protected virtual NavigationManager NavigationManager { get; init; } = default!;

	[Inject]
	protected virtual ILocalStorageService LocalStorage { get; init; } = default!;

	/// <summary>
	/// Check if the component is rendered in WASM mode.
	/// </summary>
	public virtual bool IsBrowser { get => OperatingSystem.IsBrowser(); }

	public virtual AppInfo? AppInfo => Globals.AppInfo;

	protected virtual IApiClient ApiClient
	{
		get
		{
			return ServiceProvider.GetRequiredService<IApiClient>();
		}
	}

	/// <summary>
	/// Convenience property to get the base URL for the API server.
	/// </summary>
	protected virtual string ApiBaseUrl
	{
		get
		{
			return Globals.ApiBaseUrl ?? NavigationManager.BaseUri;
		}
	}

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		if (!IsBrowser)
		{
			var taskExecutor = ServiceProvider.GetRequiredService<ITaskExecutor>();
			await taskExecutor.ExecuteOnlyOnceAsync("", () =>
			{
				// Get the app info from the configuration, once, if in Blazor Server mode
				// In WASM mode, the app info is automatically fetched from the server and stored in <see cref="Globals.AppInfo"/>
				var conf = ServiceProvider.GetRequiredService<IConfiguration>();
				Globals.AppInfo = conf.GetSection("App").Get<AppInfo>();
			});
		}
		// Add your logic here
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		// Add your logic here
	}
}
