using Bat.Blazor.App.Shared;

namespace Bat.Blazor.App.Pages;

public partial class Logout : BaseComponent
{
	private string Message { get; set; } = "Logging out, please wait...";

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await LocalStorage.RemoveItemAsync(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
		NavigationManager.NavigateTo("/", forceLoad: true);
	}
}
