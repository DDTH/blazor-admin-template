using System.Text.Json;
using Bat.Blazor.App.Shared;
using Bat.Shared.Api;

namespace Bat.Blazor.App.Pages;

public partial class RolesAdd
{
	private string RoleName { get; set; } = string.Empty;
	private string RoleDescription { get; set; } = string.Empty;

	private IEnumerable<ClaimResp>? ClaimList { get; set; }
	private IDictionary<string, bool> ClaimSelectedMap { get; set; } = new Dictionary<string, bool>();

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			ClaimSelectedMap.Clear();
			var result = await ApiClient.GetAllClaimsAsync(NavigationManager.BaseUri);
			if (result.Status == 200)
			{
				ClaimList = result.Data;
			}
			StateHasChanged();
		}
	}

	private void OnClaimChanged(string claimType, string claimValue)
	{
		var claim = $"{claimType}:{claimValue}";
		if (ClaimSelectedMap.ContainsKey(claim))
		{
			ClaimSelectedMap.Remove(claim);
		}
		else
		{
			ClaimSelectedMap.Add(claim, true);
		}
		Console.WriteLine(JsonSerializer.Serialize(ClaimSelectedMap));
	}

	private void BtnClickCancel()
	{
		NavigationManager.NavigateTo(UIGlobals.ROUTE_ROLES_LIST);
	}

	private async Task BtnClickCreate()
	{
		await Task.CompletedTask;
	}
}
