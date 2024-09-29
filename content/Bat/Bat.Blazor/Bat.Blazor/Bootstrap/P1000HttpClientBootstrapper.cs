using Bat.Shared.Bootstrap;

namespace Bat.Blazor.Bootstrap;

[Bootstrapper]
public class HttpClientBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddHttpClient();
	}
}
