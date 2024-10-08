﻿using Bat.Blazor.App.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Bat.Blazor.App.Layout;
public partial class AuthRequiredLayout : BaseLayout
{
	[CascadingParameter]
	protected virtual Task<AuthenticationState>? AuthState { get; set; }

	protected virtual IEnumerable<Claim> UserClaims { get; set; } = [];

	private string LoginUrl
	{
		get
		{
			return $"{UIGlobals.ROUTE_LOGIN}?returnUrl=/{System.Net.WebUtility.UrlEncode(NavigationManager.ToBaseRelativePath(NavigationManager.Uri))}";
		}
	}

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		// Add your logic here
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (AuthState != null)
		{
			Console.WriteLine($"[DEBUG] - {IsBrowser} / AuthRequiredLayout.OnAfterRenderAsync: validating auth stage...");
			var authState = await AuthState;
			if (authState?.User?.Identity?.IsAuthenticated ?? false)
			{
				UserClaims = authState.User.Claims;
			}
			Console.WriteLine($"[DEBUG] - {IsBrowser} / AuthRequiredLayout.OnAfterRenderAsync: validating auth stage...DONE.");
		}
		await base.OnAfterRenderAsync(firstRender);
		// Add your logic here
	}
}