using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Shared;
using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Pages;

public partial class RolesAdd
{
	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";

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
			var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
			var result = await ApiClient.GetAllClaimsAsync(authToken ?? "", NavigationManager.BaseUri);
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
	}

	private void BtnClickCancel()
	{
		NavigationManager.NavigateTo(UIGlobals.ROUTE_ROLES_LIST);
	}

	// private void CloseAlert()
	// {
	// 	AlertMessage = string.Empty;
	// 	StateHasChanged();
	// }

	private void ShowAlert(string type, string message)
	{
		AlertType = type;
		AlertMessage = message;
		StateHasChanged();
	}

	private async Task BtnClickCreate()
	{
		ShowAlert("info", "Please wait...");
		if (string.IsNullOrWhiteSpace(RoleName))
		{
			ShowAlert("warning", "Role name is required.");
			return;
		}
		var req = new CreateRoleReq
		{
			Name = RoleName.Trim(),
			Description = RoleDescription.Trim(),
			Claims = ClaimSelectedMap.Keys.Select(k => new IdentityClaim { Type = k.Split(':')[0], Value = k.Split(':')[1], }),
		};
		using (var scope = ServiceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN) ?? string.Empty;
			var resp = await ApiClient.CreateRoleAsync(req, authToken, NavigationManager.BaseUri);
			if (resp.Status != 200)
			{
				ShowAlert("danger", resp.Message!);
				return;
			}
			ShowAlert("success", "Role created successfully. Navigating to roles list...");
			await Task.Delay(1500);
			NavigationManager.NavigateTo(UIGlobals.ROUTE_ROLES_LIST);
		}
	}
}
