using Bat.Blazor.App.Layout;
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
			Id = "apps",
			Label = "Applications"
		});
	}
}
