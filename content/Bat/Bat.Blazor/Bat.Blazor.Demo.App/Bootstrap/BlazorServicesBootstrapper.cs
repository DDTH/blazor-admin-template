using Bat.Blazor.Demo.App.Services;
using Bat.Demo.Shared.Api;
using Bat.Shared.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.Demo.App.Bootstrap;

[Bootstrapper]
public class BlazorServicesBootstrapper
{
	public static void ConfigureServices(IServiceCollection services)
	{
		services.AddSingleton<IDemoApiClient, DemoApiClient>();
	}
}
