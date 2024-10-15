using Bat.Blazor.App.Helpers;
using Bat.Shared.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Pages;

public partial class Roles
{
	private int RoleIndex = 0;
	private IEnumerable<RoleResp>? RoleList { get; set; }
	private string ErrorMessage { get; set; } = string.Empty;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		RoleIndex = 0;
		if (firstRender)
		{
			var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
			var result = await ApiClient.GetAllRolesAsync(authToken ?? "", NavigationManager.BaseUri);
			if (result.Status == 200)
			{
				RoleList = result.Data;
			}
			else
			{
				ErrorMessage = result.Message ?? "Unknown error";
			}
			StateHasChanged();
		}
	}
}
