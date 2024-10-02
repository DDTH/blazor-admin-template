using Bat.Blazor.App.Shared;

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

	private void ButtonClick()
	{
		Console.WriteLine($"Button clicked: {Email} / {Password}");
	}
}
