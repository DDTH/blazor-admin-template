namespace Bat.Blazor.App.Shared;

public class UIGlobals
{
	private static readonly string PackageId = typeof(UIGlobals).Assembly.GetName().Name!;
	public static readonly string ASSET_ROOT = $"/_content/{PackageId}";
	public static readonly string COREUI_BASE = $"{ASSET_ROOT}/coreui-free-bootstrap-v5.1.0";
	public static readonly string BOOTSTRAP_ICONS_BASE = $"{ASSET_ROOT}/bootstrap-icons-1.11.3";
}
