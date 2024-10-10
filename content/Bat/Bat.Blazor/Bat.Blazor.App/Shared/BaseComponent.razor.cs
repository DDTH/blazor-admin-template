using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Layout;
using Bat.Shared.Api;
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

	[CascadingParameter(Name = "Layout")]
	protected virtual BaseLayout Layout { get; init; } = default!;

	protected virtual AppInfo? AppInfo => Layout.AppInfo;

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
		// Add your logic here
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		// Add your logic here
	}
}
