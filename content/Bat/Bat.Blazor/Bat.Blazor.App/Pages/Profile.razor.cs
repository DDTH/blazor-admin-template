using Bat.Blazor.App.Helpers;
using Bat.Shared.Api;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Bat.Blazor.App.Pages;

public partial class Profile
{
	private UserResp? User { get; set; }

	private string GivenName { get; set; } = string.Empty;
	private string FamilyName { get; set; } = string.Empty;

	private string CurrentPwd { get; set; } = string.Empty;
	private string NewPwd { get; set; } = string.Empty;
	private string ConfirmPwd { get; set; } = string.Empty;
	private string NewEmail { get; set; } = string.Empty;

	private string ProfileAlertType { get; set; } = "info";
	private string ProfileAlertMessage { get; set; } = string.Empty;
	private bool DisableUpdateProfile { get; set; } = false;

	private string EmailAlertType { get; set; } = "info";
	private string EmailAlertMessage { get; set; } = string.Empty;
	private bool DisableChangeEmail { get; set; } = false;

	private string PasswordAlertType { get; set; } = "info";
	private string PasswordAlertMessage { get; set; } = string.Empty;
	private bool DisableChangePassword { get; set; } = false;

	protected override async Task OnInitializedAsync()
	{
		using (var scope = ServiceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
			var respUser = await ApiClient.GetMyInfo(authToken!, NavigationManager.BaseUri);
			User = respUser.Data;
			GivenName = User?.GivenName ?? string.Empty;
			FamilyName = User?.FamilyName ?? string.Empty;
			NewEmail = User?.Email ?? string.Empty;
		}
	}

	private void CloseAlert()
	{
		ProfileAlertMessage = EmailAlertMessage = PasswordAlertMessage = string.Empty;
		StateHasChanged();
	}

	private void ShowAlert(string section, string type, string message)
	{
		ProfileAlertMessage = EmailAlertMessage = PasswordAlertMessage = string.Empty;
		if (section == "profile")
		{
			ProfileAlertType = type;
			ProfileAlertMessage = message;
		}
		if (section == "email")
		{
			EmailAlertType = type;
			EmailAlertMessage = message;
		}
		if (section == "password")
		{
			PasswordAlertType = type;
			PasswordAlertMessage = message;
		}
		StateHasChanged();
	}

	private async void BtnClickedSaveProfile()
	{
		DisableUpdateProfile = true;
		ShowAlert("profile", "info", "Updating profile, please wait...");
		var req = new UpdateUserProfileReq
		{
			GivenName = GivenName,
			FamilyName = FamilyName
		};
		using (var scope = ServiceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN) ?? string.Empty;
			var resp = await ApiClient.UpdateMyProfile(req, authToken, NavigationManager.BaseUri);
			DisableUpdateProfile = false;
			if (resp.Status != 200)
			{
				ShowAlert("profile", "danger", resp.Message!);
				return;
			}
			User = resp.Data;
			GivenName = User?.GivenName ?? string.Empty;
			FamilyName = User?.FamilyName ?? string.Empty;
			NewEmail = User?.Email ?? string.Empty;
			ShowAlert("profile", "success", "Profile updated successfully.");
		}
	}

	private async void BtnClickedChangeEmail()
	{
		DisableChangeEmail = true;
		ShowAlert("email", "info", "Updating email, please wait...");
		var req = new UpdateUserProfileReq
		{
			Email = NewEmail,
		};
		using (var scope = ServiceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN) ?? string.Empty;
			var resp = await ApiClient.UpdateMyProfile(req, authToken, NavigationManager.BaseUri);
			DisableChangeEmail = false;
			if (resp.Status != 200)
			{
				ShowAlert("email", "danger", resp.Message!);
				return;
			}
			User = resp.Data;
			GivenName = User?.GivenName ?? string.Empty;
			FamilyName = User?.FamilyName ?? string.Empty;
			NewEmail = User?.Email ?? string.Empty;
			ShowAlert("email", "success", "Email updated successfully.");
		}
	}

	private async void BtnClickedChangePassword()
	{
		CloseAlert();
		if (string.IsNullOrWhiteSpace(CurrentPwd) || string.IsNullOrWhiteSpace(NewPwd) || string.IsNullOrWhiteSpace(ConfirmPwd))
		{
			ShowAlert("password", "warning", "Please fill in all fields.");
			return;
		}
		CurrentPwd = CurrentPwd.Trim();
		NewPwd = NewPwd.Trim();
		ConfirmPwd = ConfirmPwd.Trim();
		if (NewPwd != ConfirmPwd)
		{
			ShowAlert("password", "warning", "New password does not match the confirmed one.");
			return;
		}
		DisableChangePassword = true;
		ShowAlert("password", "info", "Updating password, please wait...");
		using (var scope = ServiceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN) ?? string.Empty;
			var req = new ChangePasswordReq
			{
				CurrentPassword = CurrentPwd,
				NewPassword = NewPwd
			};
			var resp = await ApiClient.ChangeMyPassword(req, authToken, NavigationManager.BaseUri);
			DisableChangePassword = false;
			if (resp.Status != 200)
			{
				ShowAlert("password", "danger", resp.Message!);
				return;
			}
			CurrentPwd = NewPwd = ConfirmPwd = string.Empty;
			await localStorage.SetItemAsync(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN, resp.Data.Token);
			ShowAlert("password", "success", "Password updated successfully.");
		}
	}
}
