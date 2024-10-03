using Bat.Blazor.App.Services;
using Bat.Shared.Api;
using Bat.Shared.Bootstrap;

namespace Bat.Blazor.Bootstrap;

/// <summary>
/// Bootstrapper that registers client-services used by Blazor server.
/// </summary>
/// <remarks>
///		Bootstrappers for Blazor server and Blazor client should be separated in different projects.
/// </remarks>
[Bootstrapper]
public class ClientBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddHttpClient();
		appBuilder.Services.AddSingleton<IApiClient, ApiClient>();
	}
}
