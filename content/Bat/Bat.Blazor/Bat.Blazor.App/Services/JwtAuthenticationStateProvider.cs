using Bat.Blazor.App.Helpers;
using Bat.Shared.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
			Console.WriteLine($"[DEBUG] JwtAuthenticationStateProvider/GetAuthenticationStateAsync - {isBrowser} / authToken: {authToken}");
			if (!string.IsNullOrEmpty(authToken))
			{
				try
				{
					var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
					var principles = jwtService.ValidateToken(authToken, out _);
					if (principles != null)
					{
						principles.Claims.ToList().ForEach(c => Console.WriteLine($"[DEBUG] JwtAuthenticationStateProvider/GetAuthenticationStateAsync/User claim: {c.Type} = {c.Value}"));
						return new(principles);
					}
				} catch (Exception ex) when (ex is SecurityTokenException)
				{
				}
			}
			return new(Unauthenticated);
		}
	}

	public async Task Login(string authToken)
	{
		using (var scope = serviceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			await localStorage.SetItemAsync(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN, authToken);
		}
		NotifyStageChanged();
	}

	public async Task Logout()
	{
		using (var scope = serviceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			await localStorage.RemoveItemAsync(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
		}
		NotifyStageChanged();
	}

	public void NotifyStageChanged()
	{
		var task = GetAuthenticationStateAsync();
		NotifyAuthenticationStateChanged(task);
	}
}
