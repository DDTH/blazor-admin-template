using Bat.Shared.Api;

namespace Bat.Blazor.App;

public sealed class Globals
{
	public const string LOCAL_STORAGE_KEY_AUTH_TOKEN = "auth_token";

	/// <summary>
	/// Set to true when the server is ready to handle requests.
	/// </summary>
	public static bool Ready { get; set; } = false;

	/// <summary>
	/// Base URL for the API server, default to (WebAssemblyHostBuilder).HostEnvironment.BaseAddress
	/// </summary>
	public static string? ApiBaseUrl { get; set; }

	public static AppInfo? AppInfo { get; set; } = default;

	public static ServerInfo? ServerInfo { get; set; } = default;

	public static CryptoInfo? CryptoInfo { get; set; } = default;
}
