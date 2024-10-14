using Bat.Blazor.App.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Bat.Blazor.App.Services;

namespace Bat.Blazor.App.Pages;

public partial class Logout : BaseComponent
{
	private string Message { get; set; } = "Logging out, please wait...";

	[Inject]
	private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await ((JwtAuthenticationStateProvider)AuthenticationStateProvider).Logout();
		NavigationManager.NavigateTo(UIGlobals.ROUTE_HOME, forceLoad: false);
	}
}
