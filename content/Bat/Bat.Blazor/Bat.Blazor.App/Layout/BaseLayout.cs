﻿using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Services;
using Bat.Shared.Api;
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
	protected IServiceProvider ServiceProvider { get; init; } = default!;

	[Inject]
	protected NavigationManager NavigationManager { get; init; } = default!;

	[Inject]
	protected StateContainer StateContainer { get; init; } = default!;

	/// <summary>
	/// Check if the component is rendered in WASM mode.
	/// </summary>
	public bool IsBrowser { get => OperatingSystem.IsBrowser(); }

	public AppInfo? AppInfo { get => Globals.AppInfo; }

	public IApiClient ApiClient { get => ServiceProvider.GetRequiredService<IApiClient>(); }

	/// <summary>
	/// Convenience property to obtain the API's base URL.
	/// </summary>
	public virtual string ApiBaseUrl { get => Globals.ApiBaseUrl ?? NavigationManager.BaseUri; }

	/// <summary>
	/// Convenience method to obtain the authentication token from local storage.
	/// </summary>
	/// <returns>The authentication token, or an empty string if not found.</returns>
	protected virtual async Task<string> GetAuthTokenAsync()
	{
		using (var scope = ServiceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			return await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN) ?? string.Empty;
		}
	}

	public void Dispose()
	{
		StateContainer.OnChange -= StateHasChanged;
	}

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		StateContainer.OnChange += StateHasChanged;

		if (!IsBrowser)
		{
			var taskExecutor = ServiceProvider.GetRequiredService<ITaskExecutor>();
			await taskExecutor.ExecuteOnlyOnceAsync("FetchAppInfo", () =>
			{
				// Get the app info from the configuration, once, if in Blazor Server mode
				// In WASM mode, the app info is automatically fetched from the server and stored in <see cref="Globals.AppInfo"/>
				var conf = ServiceProvider.GetRequiredService<IConfiguration>();
				Globals.AppInfo = conf.GetSection("App").Get<AppInfo>();
				StateContainer.NotifyStateChanged();
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
