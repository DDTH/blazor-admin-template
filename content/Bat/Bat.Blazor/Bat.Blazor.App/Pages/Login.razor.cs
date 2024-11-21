using System.Text.Json;
using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Services;
using Bat.Blazor.App.Shared;
using Bat.Shared.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bat.Blazor.App.Pages;

public partial class Login : BaseComponent
{
	private CModal ModalDialog { get; set; } = default!;

	private void ShowModalNotImplemented()
	{
		ModalDialog.Open();
	}

	private string Email { get; set; } = string.Empty;
	private string Password { get; set; } = string.Empty;

	private string AlertType { get; set; } = "info";
	private string AlertMessage { get; set; } = string.Empty;
	private bool HideLoginForm { get; set; } = false;

	[Inject]
	private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

	private void CloseAlert()
	{
		AlertMessage = string.Empty;
		StateHasChanged();
	}

	private void ShowAlert(string type, string message)
	{
		AlertType = type;
		AlertMessage = message;
		StateHasChanged();
	}

	[Inject]
	private ILogger<Login> Logger { get; set; } = default!;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			// FIXME: NOT TO USE THIS IN PRODUCTION!
			// for demo purpose: automatically fill the login form
			// if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
			if (HostEnvironment.Equals(EnvironmentName.Development, StringComparison.InvariantCultureIgnoreCase))
			{
				Logger.LogCritical("DevMode - automatically fill the login form. DO NOT USE THIS IN PRODUCTION!");
				var seedUsers = await ApiClient.GetSeedUsersAsync(ApiBaseUrl);
				Logger.LogCritical("Seed users: {users}", JsonSerializer.Serialize(seedUsers));
				var user = seedUsers.Data?.FirstOrDefault();
				Email = user?.Email ?? string.Empty;
				Password = user?.Password ?? string.Empty;

				ShowAlert("info", "DevMode: Automatically fill login info.");
			}
		}
	}

	protected override async Task OnInitializedAsync()
	{
		ShowAlert("info", "Please wait...");
		await base.OnInitializedAsync();
		CloseAlert();

		Console.WriteLine($"Env: {HostEnvironment}");
	}

	private async void BtnClickLogin()
	{
		if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
		{
			ShowAlert("warning", "Please enter Email and Password to login.");
			return;
		}

		HideLoginForm = true;
		ShowAlert("info", "Authenticating, please wait...");
		var req = new AuthReq()
		{
			Email = Email,
			Password = Password
		};
		var resp = await ApiClient.LoginAsync(req, ApiBaseUrl);
		if (resp.Status != 200)
		{
			HideLoginForm = false;
			ShowAlert("danger", resp.Message!);
			return;
		}

		ShowAlert("success", "Authenticated successfully, logging in...");
		var returnUrl = QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query)
			.TryGetValue("returnUrl", out var returnUrlValue) ? returnUrlValue.FirstOrDefault("/") : "/";

		var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
		await localStorage.SetItemAsync(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN, resp.Data.Token!);
		((JwtAuthenticationStateProvider)AuthenticationStateProvider).NotifyStageChanged();

		NavigationManager.NavigateTo(returnUrl ?? "/", forceLoad: false);
	}
}
