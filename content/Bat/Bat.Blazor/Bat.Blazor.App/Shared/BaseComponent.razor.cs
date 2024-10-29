using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Layout;
using Bat.Shared.Api;
using Microsoft.AspNetCore.Components;
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

	/// <summary>
	/// Cascading the layout instance down to components.
	/// </summary>
	/// <remarks>
	/// Components can utilize shared properties defined in the layout.
	/// </remarks>
	[CascadingParameter(Name = "Layout")]
	protected virtual BaseLayout Layout { get; init; } = default!;

	// Demo: Accessing shared properties from the cascading layout instance.
	protected virtual bool IsBrowser => Layout.IsBrowser;

	// Demo: Accessing shared properties from the cascading layout instance.
	protected virtual AppInfo? AppInfo => Layout.AppInfo;

	// Demo: Accessing shared properties from the cascading layout instance.
	protected virtual IApiClient ApiClient => Layout.ApiClient;

	// Demo: Accessing shared properties from the cascading layout instance.
	protected virtual string ApiBaseUrl => Layout.ApiBaseUrl;

	/// <summary>
	/// Convenience property to construct the login url that will redirect to the current page after login.
	/// </summary>
	protected virtual string LoginUrl => $"{UIGlobals.ROUTE_LOGIN}?returnUrl=/{System.Net.WebUtility.UrlEncode(NavigationManager.ToBaseRelativePath(NavigationManager.Uri))}";

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
