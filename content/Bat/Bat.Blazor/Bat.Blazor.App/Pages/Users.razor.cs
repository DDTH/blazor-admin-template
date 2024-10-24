using System.Text.Json;
using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Shared;
using Bat.Shared.Api;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Pages;

public partial class Users
{
	private CModal ModalDialogInfo { get; set; } = default!;
	private CModal ModalDialogDelete { get; set; } = default!;

	private bool HideUI { get; set; } = false;

	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";

	private int UserIndex = 0;
	private IEnumerable<UserResp>? UserList { get; set; }
	private IDictionary<string, UserResp>? UserMap { get; set; }
	private UserResp? SelectedUser { get; set; }

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

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		UserIndex = 0;
		if (firstRender)
		{
			HideUI = true;
			ShowAlert("info", "Loading users...");
			var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
			var result = await ApiClient.GetAllUsersAsync(authToken ?? "", NavigationManager.BaseUri);
			if (result.Status == 200)
			{
				HideUI = false;
				UserList = result.Data?.OrderBy(r => r.Username);
				UserMap = UserList!.ToDictionary(user => user.Id);
				Console.WriteLine($"UserMap: {JsonSerializer.Serialize(UserMap)}");
				var queryParameters = QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query);
				var alertMessage = queryParameters.TryGetValue("alertMessage", out var alertMessageValue) ? alertMessageValue.ToString() : string.Empty;
				var alertType = queryParameters.TryGetValue("alertType", out var alertTypeValue) ? alertTypeValue.ToString() : string.Empty;
				if (!string.IsNullOrEmpty(alertMessage) && !string.IsNullOrEmpty(alertType))
				{
					ShowAlert(alertType!, alertMessage!);
				}
				else
				{
					CloseAlert();
				}
			}
			else
			{
				ShowAlert("danger", result.Message ?? "Unknown error");
			}
		}
	}

	private void BtnClickInfo(string userId)
	{
		SelectedUser = UserMap?[userId];
		ModalDialogInfo.Open();
	}

	private void BtnClickModify(string userId)
	{
		SelectedUser = UserMap?[userId];
		NavigationManager.NavigateTo(UIGlobals.ROUTE_IDENTITY_USERS_MODIFY.Replace("{id}", userId));
	}

	private void BtnClickDelete(string userId)
	{
		SelectedUser = UserMap?[userId];
		ModalDialogDelete.Open();
	}

	private async void BtnClickDeleteConfirm()
	{
		await Task.Delay(1000);
		ModalDialogDelete.Close();
		HideUI = true;
		ShowAlert("info", $"Deleting user '{SelectedUser?.Username}', please wait...");
		// var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
		// var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
		// var result = await ApiClient.DeleteUserAsync(SelectedUser?.Id ?? "", authToken ?? "", NavigationManager.BaseUri);
		// HideUI = false;
		// if (result.Status == 200)
		// {
		// 	await OnAfterRenderAsync(true);
		// 	ShowAlert("success", $"User '{SelectedUser?.Username}' deleted successfully.");
		// }
		// else
		// {
		// 	ShowAlert("danger", result.Message ?? "Unknown error");
		// }
	}

	private void BtnClickAdd()
	{
		NavigationManager.NavigateTo(UIGlobals.ROUTE_IDENTITY_USERS_ADD);
	}
}
