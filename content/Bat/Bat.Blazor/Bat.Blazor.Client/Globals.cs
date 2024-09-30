namespace Bat.Blazor.Client;

public sealed class Globals
{
	/// <summary>
	/// Set to true when the application is ready.
	/// </summary>
	public static bool Ready { get; set; } = false;

	/// <summary>
	/// Base URL for the API server, default to (WebAssemblyHostBuilder).HostEnvironment.BaseAddress
	/// </summary>
	public static string ApiBaseUrl { get; set; } = string.Empty;
}
