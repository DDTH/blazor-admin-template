using Bat.Shared.Api;

namespace Bat.Blazor.App.Pages;

public partial class RolesAdd
{
	private string RoleName { get; set; } = string.Empty;
	private string RoleDescription { get; set; } = string.Empty;

	private IEnumerable<ClaimResp>? ClaimList { get; set; }

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			var result = await ApiClient.GetAllClaimsAsync(NavigationManager.BaseUri);
			if (result.Status == 200)
			{
				ClaimList = result.Data;
			}
			StateHasChanged();
		}
	}
}
