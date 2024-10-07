using Bat.Blazor.App.Helpers;
using Bat.Shared.Api;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Shared;

/// <summary>
/// Base Razor component class that provides common properties and utility methods.
/// </summary>
public abstract class BaseComponent : ComponentBase
{
	[Inject]
	protected virtual IServiceProvider ServiceProvider { get; init; } = default!;

	[Inject]
	protected virtual NavigationManager NavigationManager { get; init; } = default!;

	[Inject]
	protected virtual LocalStorageHelper LocalStorage { get; init; } = default!;

	/// <summary>
	/// Check if the component is rendered in WASM mode.
	/// </summary>
	protected virtual bool IsBrowser { get => OperatingSystem.IsBrowser(); }

	protected virtual IApiClient ApiClient
	{
		get
		{
			return ServiceProvider.GetRequiredService<IApiClient>();
		}
	}

	private AppInfo? _appInfo;
	protected virtual AppInfo? AppInfo
	{
		get
		{
			return _appInfo ?? Globals.AppInfo;
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
			// Get the app info from the configuration if in Blazor Server mode
			// In WASM mode, the app info is automatically fetched from the server and stored in <see cref="Globals.AppInfo"/>
			var conf = ServiceProvider.GetRequiredService<IConfiguration>();
			_appInfo = conf.GetSection("App").Get<AppInfo>();
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		// Add your logic here
	}
}
