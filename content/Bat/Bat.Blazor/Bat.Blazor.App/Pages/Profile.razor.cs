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

	private void CloseAlert(string section)
	{
		if (section == "profile")
		{
			ProfileAlertMessage = string.Empty;
		}
		StateHasChanged();
	}

	private void ShowAlert(string section, string type, string message)
	{
		if (section == "profile")
		{
			ProfileAlertType = type;
			ProfileAlertMessage = message;
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
				ShowAlert("profile","danger", resp.Message!);
				return;
			}
			User = resp.Data;
			ShowAlert("profile", "success", "Profile updated successfully.");
		}
	}
}
