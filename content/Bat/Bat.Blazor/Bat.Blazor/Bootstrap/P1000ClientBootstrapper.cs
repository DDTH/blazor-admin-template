using Bat.Blazor.Client.Services;
using Bat.Shared.Api;
using Bat.Shared.Bootstrap;

namespace Bat.Blazor.Bootstrap;

[Bootstrapper]
public class ClientBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddHttpClient();
		appBuilder.Services.AddSingleton<IApiClient, ApiClient>();
	}
}
