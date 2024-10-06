using System.Security.Claims;

namespace Bat.Blazor.App.Layout;
public partial class AuthRequiredLayout : BaseLayout
{
	protected virtual IEnumerable<Claim> UserClaims { get; set; } = [];

	private string loginUrl
	{
		get
		{
			return "/login?returnUrl=/" + System.Net.WebUtility.UrlEncode(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));
		}
	}

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		// Add your logic here
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		// Add your logic here
	}
}
