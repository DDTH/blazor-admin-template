using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Bat.Blazor.App.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
	private readonly ClaimsPrincipal Unauthenticated = new(new ClaimsIdentity());

	/// <inheritdoc/>
	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var isBrowser = OperatingSystem.IsBrowser();
		Console.WriteLine($"[{isBrowser}] - JwtAuthenticationStateProvider.GetAuthenticationStateAsync called.");
		await Task.CompletedTask;
		return new(Unauthenticated);
		//return new AuthenticationState(new ClaimsPrincipal());
	}
}
