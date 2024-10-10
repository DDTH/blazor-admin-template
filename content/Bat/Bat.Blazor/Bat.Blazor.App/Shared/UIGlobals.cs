namespace Bat.Blazor.App.Shared;

public class UIGlobals
{
	private static readonly string PackageId = typeof(UIGlobals).Assembly.GetName().Name!;
	public static readonly string ASSET_ROOT = $"/_content/{PackageId}";
	public static readonly string COREUI_BASE = $"{ASSET_ROOT}/coreui-free-bootstrap-v5.1.0";
	public static readonly string BOOTSTRAP_ICONS_BASE = $"{ASSET_ROOT}/bootstrap-icons-1.11.3";

	public const string ROUTE_HOME = "/";
	public const string ROUTE_LOGIN = "/login";
	public const string ROUTE_LOGOUT = "/logout";
	public const string ROUTE_PROFILE = "/profile";

	public const string ROUTE_IDENTITY_USERS = "/admin/users";
	public const string ROUTE_IDENTITY_ROLES = "/admin/roles";

	public const string ROUTE_APPLICATIONS_LIST = "/admin/applications";
}
