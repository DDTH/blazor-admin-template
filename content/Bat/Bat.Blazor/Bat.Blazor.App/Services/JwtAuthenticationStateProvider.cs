using Bat.Blazor.App.Helpers;
using Bat.Shared.Jwt;
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
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
			Console.WriteLine($"[DEBUG] - {isBrowser} / authToken: {authToken}");
			if (!string.IsNullOrEmpty(authToken))
			{
				var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
				var principles = jwtService.ValidateToken(authToken, out _);
				if (principles != null)
				{
					return new(new ClaimsPrincipal(principles));
				}
			}
			return new(Unauthenticated);
		}
	}
}
