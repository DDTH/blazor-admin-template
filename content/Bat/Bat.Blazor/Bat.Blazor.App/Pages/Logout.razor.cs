using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Pages;

public partial class Logout : BaseComponent
{
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
		await localStorage.RemoveItemAsync(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);

		NavigationManager.NavigateTo(UIGlobals.ROUTE_LANDING, forceLoad: true);
	}
}
