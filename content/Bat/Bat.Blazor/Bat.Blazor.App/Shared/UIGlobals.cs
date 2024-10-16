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

	public const string ROUTE_BASE = "/admin";
	public const string ROUTE_CATCHALL = ROUTE_BASE+"/{*route:nonfile}";

	public const string ROUTE_IDENTITY_USERS = $"{ROUTE_BASE}/users";
	public const string ROUTE_IDENTITY_ROLES = $"{ROUTE_BASE}/roles";

	public const string ROUTE_APPLICATIONS_LIST = $"{ROUTE_BASE}/applications";
	public const string ROUTE_APPLICATIONS_ADD = $"{ROUTE_BASE}/applications/add";
	public const string ROUTE_APPLICATION_DELETE =$"{ROUTE_BASE}/applications/delete/{{id}}";

	public const string ROUTE_ROLES_LIST = $"{ROUTE_BASE}/roles";
	public const string ROUTE_ROLES_ADD = $"{ROUTE_BASE}/roles/add";
	public const string ROUTE_ROLES_DELETE = $"{ROUTE_BASE}/roles/delete/"+"{id}";
	public const string ROUTE_ROLES_MODIFY = $"{ROUTE_BASE}/roles/modify/"+"{id}";
}
