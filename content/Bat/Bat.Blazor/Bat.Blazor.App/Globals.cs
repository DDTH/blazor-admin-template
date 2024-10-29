using Bat.Shared.Api;

namespace Bat.Blazor.App;

public sealed class Globals
{
	public const string LOCAL_STORAGE_KEY_AUTH_TOKEN = "auth_token";

	/// <summary>
	/// In WASM mode, this is the unique instance ID for the client.
	/// </summary>
	public static readonly string INSTANCE_ID = Guid.NewGuid().ToString();

	/// <summary>
	/// Set to true when the server is ready to handle requests.
	/// </summary>
	public static bool Ready { get; set; } = false;

	public const string CONF_KEY_API_BASE_URL = "API:BaseUrl";
	/// <summary>
	/// Base URL for the API server. This value will be populated during bootstrapping:
	/// - From key "API:BaseUrl" in appsettings.json
	/// - If empty, Default to (WebAssemblyHostBuilder).HostEnvironment.BaseAddress (Blazor WASM mode only!)
	/// </summary>
	public static string? ApiBaseUrl { get; set; }

	public static AppInfo? AppInfo { get; set; } = default;

	public static ServerInfo? ServerInfo { get; set; } = default;

	public static CryptoInfo? CryptoInfo { get; set; } = default;
}
