using Bat.Blazor.App.Shared;
using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Components;

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
			var result = await ApiClient.GetAllClaimsAsync(await GetAuthTokenAsync(), ApiBaseUrl);
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
		NavigationManager.NavigateTo(UIGlobals.ROUTE_IDENTITY_ROLES);
	}

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
		var req = new CreateOrUpdateRoleReq
		{
			Name = RoleName.Trim(),
			Description = RoleDescription.Trim(),
			Claims = ClaimSelectedMap.Keys.Select(k => new IdentityClaim { Type = k.Split(':')[0], Value = k.Split(':')[1], }),
		};
		var resp = await ApiClient.CreateRoleAsync(req, await GetAuthTokenAsync(), ApiBaseUrl);
		if (resp.Status != 200)
		{
			ShowAlert("danger", resp.Message!);
			return;
		}
		ShowAlert("success", "Role created successfully. Navigating to roles list...");
		var passAlertMessage = $"Role '{req.Name}' created successfully.";
		var passAlertType = "success";
		await Task.Delay(500);
		NavigationManager.NavigateTo($"{UIGlobals.ROUTE_IDENTITY_ROLES}?alertMessage={passAlertMessage}&alertType={passAlertType}");
	}
}
