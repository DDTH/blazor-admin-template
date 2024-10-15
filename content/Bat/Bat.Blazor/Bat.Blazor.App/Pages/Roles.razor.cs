using Bat.Shared.Api;

namespace Bat.Blazor.App.Pages;

public partial class Roles
{
	private IEnumerable<RoleResp>? RoleList { get; set; }
	private string ErrorMessage { get; set; } = string.Empty;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			var result = await ApiClient.GetAllRolesAsync();
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
