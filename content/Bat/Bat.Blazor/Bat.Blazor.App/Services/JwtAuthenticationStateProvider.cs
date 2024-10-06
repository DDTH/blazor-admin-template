using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Bat.Blazor.App.Services;

public class JwtAuthenticationStateProvider(IServiceProvider serviceProvider) : AuthenticationStateProvider
{
	private readonly ClaimsPrincipal Unauthenticated = new(new ClaimsIdentity());

	/// <inheritdoc/>
	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		using (var scope = serviceProvider.CreateScope())
		{
			var isBrowser = OperatingSystem.IsBrowser();
			var localStorage = scope.ServiceProvider.GetRequiredService<ILocalStorageService>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
			Console.WriteLine($"[{isBrowser}]-JwtAuthenticationStateProvider.GetAuthenticationStateAsync: {authToken}");
			await Task.CompletedTask;
			return new(Unauthenticated);
		}
	}
}
