// using Bat.Blazor.App.Shared;
// using Bat.Shared.Api;
// using Microsoft.AspNetCore.Components;

// namespace Bat.Blazor.App.Pages;

// public partial class ApplicationsAdd
// {
// 	private string AlertMessage { get; set; } = string.Empty;
// 	private string AlertType { get; set; } = "info";
// 	private bool HideUI { get; set; } = false;

// 	private string DisplayName { get; set; } = string.Empty;
// 	private string PublicKeyPEM { get; set; } = string.Empty;

// 	private void BtnClickCancel()
// 	{
// 		NavigationManager.NavigateTo(UIGlobals.ROUTE_APPLICATIONS);
// 	}

// 	private void ShowAlert(string type, string message)
// 	{
// 		AlertType = type;
// 		AlertMessage = message;
// 		StateHasChanged();
// 	}

// 	private async Task BtnClickCreate()
// 	{
// 		HideUI = true;
// 		ShowAlert("info", "Please wait...");

// 		// Validate display name
// 		if (string.IsNullOrWhiteSpace(DisplayName))
// 		{
// 			HideUI = false;
// 			ShowAlert("warning", "Display name is required.");
// 			return;
// 		}

// 		var req = new CreateOrUpdateAppReq
// 		{
// 			DisplayName = DisplayName.Trim(),
// 			PublicKeyPEM = PublicKeyPEM.Trim(),
// 		};
// 		var resp = await ApiClient.CreateAppAsync(req, await GetAuthTokenAsync(), ApiBaseUrl);
// 		if (resp.Status != 200)
// 		{
// 			HideUI = false;
// 			ShowAlert("danger", resp.Message!);
// 			return;
// 		}
// 		ShowAlert("success", "Application created successfully. Navigating to applications list...");
// 		var passAlertMessage = $"Application '{req.DisplayName}' created successfully.";
// 		var passAlertType = "success";
// 		await Task.Delay(500);
// 		NavigationManager.NavigateTo($"{UIGlobals.ROUTE_APPLICATIONS}?alertMessage={passAlertMessage}&alertType={passAlertType}");
// 	}
// }
