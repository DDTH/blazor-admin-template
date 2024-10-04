using Bat.Blazor.App.Shared;
using Bat.Shared.Api;
using System.Text.Json;

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

	protected override async Task OnInitializedAsync()
	{
		ShowAlert("info", "Please wait...");
		await base.OnInitializedAsync();
		CloseAlert();
	}

	private async void BtnClickLogin()
	{
		if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
		{
			ShowAlert("warning", "Please enter Email and Password to login.");
			return;
		}
		ShowAlert("info", "Authenticating, please wait...");
		var req = new LoginReq()
		{
			Email = Email,
			Password = Password
		};
		var resp = await ApiClient.LoginAsync(req, ApiBaseUrl);
		Console.WriteLine($"Login response: {JsonSerializer.Serialize(resp)}");
		if (resp.Status != 200)
		{
			ShowAlert("danger", resp.Message!);
			return;
		}
	}
}
