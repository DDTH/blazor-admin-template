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
	/// <summary>
	/// Check if the component is rendered in WASM mode.
	/// </summary>
	protected virtual bool IsBrowser { get => OperatingSystem.IsBrowser(); }

	[Inject]
	protected virtual IServiceProvider ServiceProvider { get; init; } = default!;

	[Inject]
	protected virtual NavigationManager NavigationManager { get; init; } = default!;

	[Inject]
	protected virtual ILocalStorageService LocalStorage { get; init; } = default!;

	private AppInfo? _appInfo;
	protected virtual AppInfo? AppInfo
	{
		get
		{
			return _appInfo ?? Globals.AppInfo;
		}
	}

	protected virtual IApiClient ApiClient
	{
		get
		{
			return ServiceProvider.GetRequiredService<IApiClient>();
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
		// Add your logic here
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		// Add your logic here
	}
}
