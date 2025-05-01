// using Bat.Blazor.App.Shared;
// using Microsoft.AspNetCore.WebUtilities;

// namespace Bat.Blazor.Demo.App.Pages;

// public partial class Applications
// {
// 	private CModal ModalDialogInfo { get; set; } = default!;
// 	private CModal ModalDialogDelete { get; set; } = default!;

// 	private bool HideUI { get; set; } = false;

// 	private string AlertMessage { get; set; } = string.Empty;
// 	private string AlertType { get; set; } = "info";

// 	private int AppIndex = 0;
// 	private IEnumerable<AppResp>? AppList { get; set; }
// 	private IDictionary<string, AppResp>? AppMap { get; set; }
// 	private AppResp? SelectedApp { get; set; }

// 	private void CloseAlert()
// 	{
// 		AlertMessage = string.Empty;
// 		StateHasChanged();
// 	}

// 	private void ShowAlert(string type, string message)
// 	{
// 		AlertType = type;
// 		AlertMessage = message;
// 		StateHasChanged();
// 	}

// 	protected override async Task OnAfterRenderAsync(bool firstRender)
// 	{
// 		await base.OnAfterRenderAsync(firstRender);
// 		AppIndex = 0;
// 		if (firstRender)
// 		{
// 			HideUI = true;
// 			ShowAlert("info", "Loading applications...");
// 			var result = await ApiClient.GetAllAppsAsync(await GetAuthTokenAsync(), ApiBaseUrl);
// 			if (result.Status == 200)
// 			{
// 				HideUI = false;
// 				AppList = result.Data?.OrderBy(a => a.DisplayName);
// 				AppMap = AppList!.ToDictionary(app => app.Id);
// 				var queryParameters = QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query);
// 				var alertMessage = queryParameters.TryGetValue("alertMessage", out var alertMessageValue) ? alertMessageValue.ToString() : string.Empty;
// 				var alertType = queryParameters.TryGetValue("alertType", out var alertTypeValue) ? alertTypeValue.ToString() : string.Empty;
// 				if (!string.IsNullOrEmpty(alertMessage) && !string.IsNullOrEmpty(alertType))
// 				{
// 					ShowAlert(alertType!, alertMessage!);
// 				}
// 				else
// 				{
// 					CloseAlert();
// 				}
// 			}
// 			else
// 			{
// 				ShowAlert("danger", result.Message ?? "Unknown error");
// 			}
// 		}
// 	}

// 	private void BtnClickInfo(string appId)
// 	{
// 		SelectedApp = AppMap?[appId];
// 		ModalDialogInfo.Open();
// 	}

// 	private void BtnClickModify(string appId)
// 	{
// 		SelectedApp = AppMap?[appId];
// 		NavigationManager.NavigateTo(UIGlobals.ROUTE_APPLICATIONS_MODIFY.Replace("{id}", appId));
// 	}

// 	private void BtnClickDelete(string appId)
// 	{
// 		SelectedApp = AppMap?[appId];
// 		ModalDialogDelete.Open();
// 	}

// 	private async void BtnClickDeleteConfirm()
// 	{
// 		ModalDialogDelete.Close();
// 		HideUI = true;
// 		ShowAlert("info", $"Deleting application '{SelectedApp?.DisplayName}', please wait...");
// 		var result = await ApiClient.DeleteAppAsync(SelectedApp?.Id ?? string.Empty, await GetAuthTokenAsync(), ApiBaseUrl);
// 		HideUI = false;
// 		if (result.Status == 200)
// 		{
// 			await OnAfterRenderAsync(true);
// 			ShowAlert("success", $"Application '{SelectedApp?.DisplayName}' deleted successfully.");
// 		}
// 		else
// 		{
// 			ShowAlert("danger", result.Message ?? "Unknown error");
// 		}
// 	}

// 	private void BtnClickAdd()
// 	{
// 		NavigationManager.NavigateTo(UIGlobals.ROUTE_APPLICATIONS_ADD);
// 	}
// }
