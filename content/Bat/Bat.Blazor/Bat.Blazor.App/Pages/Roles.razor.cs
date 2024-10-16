using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Shared;
using Bat.Shared.Api;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Bat.Blazor.App.Pages;

public partial class Roles
{
	private CModal ModalDialogInfo { get; set; } = default!;

	private int RoleIndex = 0;
	private IEnumerable<RoleResp>? RoleList { get; set; }
	private IDictionary<string, RoleResp>? RoleMap { get; set; }
	private RoleResp? SelectedRole { get; set; }

	private string ErrorMessage { get; set; } = string.Empty;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		base.OnAfterRender(firstRender);
		RoleIndex = 0;
		if (firstRender)
		{
			var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
			var result = await ApiClient.GetAllRolesAsync(authToken ?? "", NavigationManager.BaseUri);
			if (result.Status == 200)
			{
				RoleList = result.Data;
				RoleMap = RoleList!.ToDictionary(role => role.Id);
			}
			else
			{
				ErrorMessage = result.Message ?? "Unknown error";
			}
			StateHasChanged();
		}
	}

	private void BtnClickInfo(string roleId)
	{
		SelectedRole = RoleMap?[roleId];
		Console.WriteLine($"[BtnClickInfo] Role ID: {roleId} -> {JsonSerializer.Serialize(SelectedRole)}");
		ModalDialogInfo.Open();
	}

	private void BtnClickModify(string roleId)
	{
		Console.WriteLine($"[BtnClickModify] Role ID: {roleId} -> {RoleMap?[roleId]}");
	}

	private void BtnClickDelete(string roleId)
	{
		Console.WriteLine($"[BtnClickDelete] Role ID: {roleId} -> {RoleMap?[roleId]}");
	}
}
