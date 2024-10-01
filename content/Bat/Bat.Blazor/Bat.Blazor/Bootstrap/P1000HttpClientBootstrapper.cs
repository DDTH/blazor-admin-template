using Bat.Shared.Bootstrap;

namespace Bat.Blazor.Bootstrap;

/// <summary>
/// Bootstrapper that registers HttpClient to the service registry.
/// </summary>
[Bootstrapper]
public class HttpClientBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddHttpClient();
	}
}
