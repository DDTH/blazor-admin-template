using Bat.Blazor.App.Shared;
using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Components;

namespace Bat.Blazor.App.Pages;

public partial class UsersAdd
{
	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";

	private string UserName { get; set; } = string.Empty;
	private string UserEmail { get; set; } = string.Empty;
	private string UserFamilyName { get; set; } = string.Empty;
	private string UserGivenName { get; set; } = string.Empty;
	private string UserPassword { get; set; } = string.Empty;
	private string UserConfirmPassword { get; set; } = string.Empty;

	private IEnumerable<RoleResp>? RoleList { get; set; }
	private IDictionary<string, bool> RoleSelectedMap { get; set; } = new Dictionary<string, bool>();

	private IEnumerable<ClaimResp>? ClaimList { get; set; }
	private IDictionary<string, bool> ClaimSelectedMap { get; set; } = new Dictionary<string, bool>();

	private void ShowAlert(string type, string message)
	{
		AlertType = type;
		AlertMessage = message;
		StateHasChanged();
	}

	private void CloseAlert()
	{
		AlertMessage = string.Empty;
		StateHasChanged();
	}

	private async Task<ApiResp<IEnumerable<RoleResp>>> GetAllRolesAsync(string authToken)
	{
		ShowAlert("info", "Loading roles, please wait...");
		RoleSelectedMap.Clear();
		var result = await ApiClient.GetAllRolesAsync(authToken, ApiBaseUrl);
		if (result.Status == 200)
		{
			RoleList = result.Data;
		}
		return result;
	}

	private async Task<ApiResp<IEnumerable<ClaimResp>>> GetAllClaimsAsync(string authToken)
	{
		ShowAlert("info", "Loading claims, please wait...");
		ClaimSelectedMap.Clear();
		var result = await ApiClient.GetAllClaimsAsync(authToken, ApiBaseUrl);
		if (result.Status == 200)
		{
			ClaimList = result.Data;
		}
		return result;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			var roleResult = await GetAllRolesAsync(await GetAuthTokenAsync());
			if (roleResult.Status != 200)
			{
				ShowAlert("danger", roleResult.Message!);
				return;
			}

			var claimResult = await GetAllClaimsAsync(await GetAuthTokenAsync());
			if (claimResult.Status != 200)
			{
				ShowAlert("danger", claimResult.Message!);
				return;
			}

			CloseAlert();
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

	private void OnRoleChanged(string roleId)
	{
		if (RoleSelectedMap.ContainsKey(roleId))
		{
			RoleSelectedMap.Remove(roleId);
		}
		else
		{
			RoleSelectedMap.Add(roleId, true);
		}
	}

	private void BtnClickCancel()
	{
		NavigationManager.NavigateTo(UIGlobals.ROUTE_IDENTITY_USERS);
	}

	private async Task BtnClickCreate()
	{
		ShowAlert("info", "Please wait...");
		if (string.IsNullOrWhiteSpace(UserName))
		{
			ShowAlert("warning", "Username is required.");
			return;
		}
		if (string.IsNullOrWhiteSpace(UserEmail))
		{
			ShowAlert("warning", "Email is required.");
			return;
		}
		if (string.IsNullOrWhiteSpace(UserPassword))
		{
			ShowAlert("warning", "Password is required.");
			return;
		}
		if (!UserPassword.Equals(UserConfirmPassword, StringComparison.InvariantCulture))
		{
			ShowAlert("warning", "Password does not match the confirmed one.");
			return;
		}
		var req = new CreateOrUpdateUserReq
		{
			Username = UserName.ToLower().Trim(),
			Email = UserEmail.ToLower().Trim(),
			Password = UserPassword.Trim(),
			GivenName = UserGivenName.Trim(),
			FamilyName = UserFamilyName.Trim(),
			Roles = RoleSelectedMap.Keys.Select(k => k),
			Claims = ClaimSelectedMap.Keys.Select(k => new IdentityClaim { Type = k.Split(':')[0], Value = k.Split(':')[1], }),
		};
		var resp = await ApiClient.CreateUserAsync(req, await GetAuthTokenAsync(), ApiBaseUrl);
		if (resp.Status != 200)
		{
			ShowAlert("danger", resp.Message!);
			return;
		}
		ShowAlert("success", "User created successfully. Navigating to users list...");
		var passAlertMessage = $"User '{req.Username}' created successfully.";
		var passAlertType = "success";
		await Task.Delay(500);
		NavigationManager.NavigateTo($"{UIGlobals.ROUTE_IDENTITY_USERS}?alertMessage={passAlertMessage}&alertType={passAlertType}");
	}
}
