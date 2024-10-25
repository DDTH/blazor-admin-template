using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Shared;
using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Pages;

public partial class UsersModify
{
	[Parameter]
	public string Id { get; set; } = string.Empty;

	private string AlertMessage { get; set; } = string.Empty;
	private string AlertType { get; set; } = "info";
	private bool HideUI { get; set; } = false;

	private string UserName { get; set; } = string.Empty;
	private string UserEmail { get; set; } = string.Empty;
	private string UserFamilyName { get; set; } = string.Empty;
	private string UserGivenName { get; set; } = string.Empty;

	private UserResp? SelectedUser { get; set; }
	private IEnumerable<RoleResp>? RoleList { get; set; }
	private IDictionary<string, bool> RoleSelectedMap { get; set; } = new Dictionary<string, bool>();

	private IEnumerable<ClaimResp>? ClaimList { get; set; }
	private IDictionary<string, bool> ClaimSelectedMap { get; set; } = new Dictionary<string, bool>();

	private async Task<UserResp?> LoadUserAsync(string id, string authToken)
	{
		HideUI = true;
		ShowAlert("info", "Loading user details. Please wait...");
		var result = await ApiClient.GetUserAsync(id, authToken, NavigationManager.BaseUri);
		if (result.Status == 200)
		{
			return result.Data;
		}
		ShowAlert("danger", result.Message!);
		return null;
	}

	private async Task<ApiResp<IEnumerable<ClaimResp>>> LoadAllClaimsAsync(string authToken)
	{
		HideUI = true;
		ShowAlert("info", "Loading claims, please wait...");
		ClaimSelectedMap.Clear();
		var result = await ApiClient.GetAllClaimsAsync(authToken, NavigationManager.BaseUri);
		if (result.Status == 200)
		{
			ClaimList = result.Data;
		}
		return result;
	}

	private async Task<ApiResp<IEnumerable<RoleResp>>> LoadAllRolesAsync(string authToken)
	{
		HideUI = true;
		ShowAlert("info", "Loading roles, please wait...");
		RoleSelectedMap.Clear();
		var result = await ApiClient.GetAllRolesAsync(authToken, NavigationManager.BaseUri);
		if (result.Status == 200)
		{
			RoleList = result.Data;
		}
		return result;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			HideUI = true;
			ShowAlert("info", "Loading role details. Please wait...");
			var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);

			SelectedUser = await LoadUserAsync(Id, authToken ?? "");
			if (SelectedUser == null)
			{
				return;
			}
			UserName = SelectedUser?.Username ?? string.Empty;
			UserEmail = SelectedUser?.Email ?? string.Empty;
			UserFamilyName = SelectedUser?.FamilyName ?? string.Empty;
			UserGivenName = SelectedUser?.GivenName ?? string.Empty;

			var roleResult = await LoadAllRolesAsync(authToken ?? "");
			if (roleResult.Status != 200)
			{
				ShowAlert("danger", roleResult.Message!);
				return;
			}
			if (SelectedUser?.Roles != null)
			{
				foreach (var role in SelectedUser?.Roles!)
				{
					RoleSelectedMap.Add(role.Id, true);
				}
			}

			var claimResult = await LoadAllClaimsAsync(authToken ?? "");
			if (claimResult.Status != 200)
			{
				ShowAlert("danger", claimResult.Message!);
				return;
			}
			if (SelectedUser?.Claims != null)
			{
				foreach (var claim in SelectedUser?.Claims!)
				{
					ClaimSelectedMap.Add($"{claim.ClaimType}:{claim.ClaimValue}", true);
				}
			}

			HideUI = false;
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

	private void ShowAlert(string type, string message)
	{
		AlertType = type;
		AlertMessage = message;
		StateHasChanged();
	}

	private void CloseAlert()
	{
		AlertType = AlertMessage = string.Empty;
		StateHasChanged();
	}

	private async Task BtnClickSave()
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
		var req = new CreateOrUpdateUserReq
		{
			Username = UserName.ToLower().Trim(),
			Email = UserEmail.ToLower().Trim(),
			GivenName = UserGivenName.Trim(),
			FamilyName = UserFamilyName.Trim(),
			Roles = RoleSelectedMap.Keys.Select(k => k),
			Claims = ClaimSelectedMap.Keys.Select(k => new IdentityClaim { Type = k.Split(':')[0], Value = k.Split(':')[1], }),
		};
		using (var scope = ServiceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN) ?? string.Empty;
			var resp = await ApiClient.UpdateUserAsync(Id, req, authToken, NavigationManager.BaseUri);
			if (resp.Status != 200)
			{
				ShowAlert("danger", resp.Message!);
				return;
			}
			ShowAlert("success", "User updated successfully. Navigating to users list...");
			var passAlertMessage = $"User '{Id}' updated successfully.";
			var passAlertType = "success";
			await Task.Delay(500);
			NavigationManager.NavigateTo($"{UIGlobals.ROUTE_IDENTITY_USERS}?alertMessage={passAlertMessage}&alertType={passAlertType}");
		}
	}
}
