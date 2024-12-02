using Bat.Blazor.App.Helpers;
using Bat.Blazor.App.Services;
using Bat.Blazor.App.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Pages.ExternalAuth;

public partial class LoginMicrosoft
{
	private string AlertType { get; set; } = string.Empty;
	private string AlertMessage { get; set; } = string.Empty;
	private bool ShowReturnLinks { get; set; } = false;

	[Inject]
	private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

	private void ShowAlert(string type, string message)
	{
		AlertType = type;
		AlertMessage = message;
		StateHasChanged();
	}

	protected override async Task OnInitializedAsync()
	{
		ShowAlert("waiting", "Logging in, please wait...");
		await base.OnInitializedAsync();

		var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
		var queryParams = QueryHelpers.ParseQuery(uri.Query);
		var authData = queryParams.ToDictionary(x => x.Key, x => x.Value.ToString()) ?? [];
		authData["redirect_uri"] = new UriBuilder(NavigationManager.BaseUri)
		{
			Path = UIGlobals.ROUTE_LOGIN_EXTERNAL_MICROSOFT,
		}.ToString();
		var apiResult = await ApiClient.ExternalLoginAsync(new Bat.Shared.Api.ExternalAuthReq
		{
			Provider = "Microsoft",
			AuthData = authData
		}, ApiBaseUrl);
		if (apiResult.Status != 200)
		{
			ShowReturnLinks = true;
			ShowAlert("danger", $"{apiResult.Status}: {apiResult.Message}");
			return;
		}

		ShowAlert("success", "Authenticated successfully, logging in...⏳");
		ShowReturnLinks = true;
		var localStorage = ServiceProvider.GetRequiredService<LocalStorageHelper>();
		await localStorage.SetItemAsync(Globals.LOCAL_STORAGE_KEY_AUTH_TOKEN, apiResult.Data.Token!);
		((JwtAuthenticationStateProvider)AuthenticationStateProvider).NotifyStageChanged();
		var returnUrl = apiResult.Data.ReturnUrl ?? UIGlobals.ROUTE_HOME;
		NavigationManager.NavigateTo(returnUrl, forceLoad: false);
	}
}
