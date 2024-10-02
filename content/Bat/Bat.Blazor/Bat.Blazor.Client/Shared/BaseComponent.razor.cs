using Bat.Shared.Api;
using Microsoft.AspNetCore.Components;

namespace Bat.Blazor.Client.Shared;

public abstract class BaseComponent : ComponentBase
{
	[Inject]
	protected virtual IServiceProvider ServiceProvider { get; init; } = default!;

	[Inject]
	protected virtual NavigationManager NavigationManager { get; init; } = default!;

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

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		if (!IsBrowser)
		{
			var conf = ServiceProvider.GetRequiredService<IConfiguration>();
			_appInfo = conf.GetSection("App").Get<AppInfo>();
		}
	}

	//protected override async Task OnAfterRenderAsync(bool firstRender)
	//{
	//	await base.OnAfterRenderAsync(firstRender);
	//	AppInfo = Globals.AppInfo;
	//}
}
