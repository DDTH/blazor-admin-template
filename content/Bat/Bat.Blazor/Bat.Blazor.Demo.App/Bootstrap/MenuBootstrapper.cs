using Bat.Blazor.App.Layout;
using Bat.Blazor.Demo.App.Shared;
using Bat.Shared.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.Demo.App.Bootstrap;

[Bootstrapper]
public class MenuBootstrapper
{
	public static void ConfigureServices(IServiceCollection _)
	{
		Sidebar.AddOrReplaceSection(new Sidebar.SidebarSection
		{
			Id = "demo",
			Label = "Demo",
			Items = [
				new Sidebar.SidebarItem
				{
					Id = "apps",
					Label = "Applications",
					Icon = "cil-apps",
					Url = DemoUIGlobals.ROUTE_APPLICATIONS,
				}
			],
		});
	}
}
