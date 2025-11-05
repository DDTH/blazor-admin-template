using Bat.Demo.Shared.Identity;
using Bat.Shared.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.Demo.App.Bootstrap;

[Bootstrapper]
public class AuthBootstrapper
{
	public static void ConfigureServices(IServiceCollection services)
	{
		// set up authorization
		services.AddAuthorizationCore(c =>
		{
			// Configurate authorization policies
			c.AddPolicy(DemoPolicies.POLICY_NAME_ADMIN_ROLE_OR_APPLICATION_MANAGER, DemoPolicies.POLICY_ADMIN_ROLE_OR_APPLICATION_MANAGER);
			c.AddPolicy(DemoPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_APP_PERM, DemoPolicies.POLICY_ADMIN_ROLE_OR_CREATE_APP_PERM);
			c.AddPolicy(DemoPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM, DemoPolicies.POLICY_ADMIN_ROLE_OR_MODIFY_APP_PERM);
			c.AddPolicy(DemoPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_APP_PERM, DemoPolicies.POLICY_ADMIN_ROLE_OR_DELETE_APP_PERM);
		});
	}
}
