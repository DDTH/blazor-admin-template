using Bat.Blazor.App.Helpers;
using Bat.Shared.Api;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Bat.Blazor.App.Pages;

public partial class Profile
{
	private UserResp? User { get; set; }

	private string GivenName { get; set; } = string.Empty;
	private string FamilyName { get; set; } = string.Empty;

	private string CurrentPwd { get; set; } = string.Empty;
	private string NewPwd { get; set; } = string.Empty;
	private string ConfirmPwd { get; set; } = string.Empty;
	private string NewEmail { get; set; } = string.Empty;

	protected override async Task OnInitializedAsync()
	{
		using (var scope = ServiceProvider.CreateScope())
		{
			var localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageHelper>();
			var authToken = await localStorage.GetItemAsync<string>(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN);
			var respUser = await ApiClient.GetMyInfo(authToken!, NavigationManager.BaseUri);
			User = respUser.Data;
			GivenName = User?.GivenName ?? string.Empty;
			FamilyName = User?.FamilyName ?? string.Empty;
			NewEmail = User?.Email ?? string.Empty;
			Console.WriteLine($"[DEBUG] - Profile/OnInitializedAsync/authToken: {JsonSerializer.Serialize(User)}");
		}

		//var response = await _api.GetMyInfo();
		//if (response.Status == 200)
		//{
		//	_user = response.Data;
		//}
		//else
		//{
		//}
	}
}
