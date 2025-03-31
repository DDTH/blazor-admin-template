using System.Net;
using Bat.Blazor.App.Shared;
using Microsoft.AspNetCore.WebUtilities;

namespace Bat.Blazor.App.Pages.ExternalAuth;

public abstract class LoginExternalBase : BaseComponent
{
	protected IDictionary<string, string> ParseQueryParams(bool urlDecode = true, bool htmlDecode = false)
	{
		var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
		var queryParams = QueryHelpers.ParseQuery(uri.Query);
		return queryParams.ToDictionary(x => x.Key, x =>
		{
			var value = x.Value.ToString();
			if (urlDecode)
			{
				value = WebUtility.UrlDecode(value);
			}
			if (htmlDecode)
			{
				value = WebUtility.HtmlDecode(value);
			}
			return value;
		}) ?? [];
	}
}
