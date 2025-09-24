using Bat.Blazor.App.Shared;

namespace Bat.Blazor.Demo.App.Shared;

public class DemoUIGlobals : UIGlobals
{
	public const string ROUTE_APPLICATIONS = $"{ROUTE_BASE}/applications";
	public const string ROUTE_APPLICATIONS_ADD = $"{ROUTE_BASE}/applications/add";
	public const string ROUTE_APPLICATIONS_DELETE = $"{ROUTE_BASE}/applications/delete/{{id}}";
	public const string ROUTE_APPLICATIONS_MODIFY = $"{ROUTE_BASE}/applications/modify/{{id}}";

	public const string ROUTE_UPLOAD_SINGLE = $"{ROUTE_BASE}/upload_single";
}
