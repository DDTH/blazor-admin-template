namespace Bat.Blazor.App.Shared;

public class UIGlobals
{
	private static readonly string PackageId = typeof(UIGlobals).Assembly.GetName().Name!;
	public static readonly string ASSET_ROOT = $"/_content/{PackageId}";
	public static readonly string COREUI_BASE = $"{ASSET_ROOT}/coreui-free-bootstrap-v5.1.0";
	public static readonly string BOOTSTRAP_ICONS_BASE = $"{ASSET_ROOT}/bootstrap-icons-1.11.3";

	public const string ROUTE_LANDING = "/";
	public const string ROUTE_LOGIN = "/login";
	public const string ROUTE_LOGOUT = "/logout";
	public const string ROUTE_PROFILE = "/profile";

	public const string ROUTE_LOGIN_EXTERNAL_LINKEDIN = "/external-auth/linkedin";
	public const string ROUTE_LOGIN_EXTERNAL_MICROSOFT = "/external-auth/microsoft";
	public const string ROUTE_LOGIN_EXTERNAL_GITHUB = "/external-auth/github";

	public const string ROUTE_BASE = "/admin";
	public const string ROUTE_HOME = $"{ROUTE_BASE}/";
	public const string ROUTE_CATCHALL = ROUTE_BASE + "/{*route:nonfile}";

	public const string ROUTE_IDENTITY_USERS = $"{ROUTE_BASE}/users";
	public const string ROUTE_IDENTITY_USERS_ADD = $"{ROUTE_BASE}/users/add";
	public const string ROUTE_IDENTITY_USERS_DELETE = $"{ROUTE_BASE}/users/delete/{{id}}";
	public const string ROUTE_IDENTITY_USERS_MODIFY = $"{ROUTE_BASE}/users/modify/{{id}}";

	public const string ROUTE_IDENTITY_ROLES = $"{ROUTE_BASE}/roles";
	public const string ROUTE_IDENTITY_ROLES_ADD = $"{ROUTE_BASE}/roles/add";
	public const string ROUTE_IDENTITY_ROLES_DELETE = $"{ROUTE_BASE}/roles/delete/{{id}}";
	public const string ROUTE_IDENTITY_ROLES_MODIFY = $"{ROUTE_BASE}/roles/modify/{{id}}";

	public const string ROUTE_APPLICATIONS = $"{ROUTE_BASE}/applications";
	public const string ROUTE_APPLICATIONS_ADD = $"{ROUTE_BASE}/applications/add";
	public const string ROUTE_APPLICATIONS_DELETE = $"{ROUTE_BASE}/applications/delete/{{id}}";
	public const string ROUTE_APPLICATIONS_MODIFY = $"{ROUTE_BASE}/applications/modify/{{id}}";
}
