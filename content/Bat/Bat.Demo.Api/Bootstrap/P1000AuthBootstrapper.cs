using Bat.Demo.Shared.Identity;
using Bat.Shared.Bootstrap;

namespace Bat.Demo.Api.Bootstrap;

[Bootstrapper]
public class AuthBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		// Configurate authorization policies
		appBuilder.Services.AddAuthorization(c =>
		{
			c.AddPolicy(DemoPolicies.POLICY_NAME_ADMIN_ROLE_OR_APPLICATION_MANAGER, DemoPolicies.POLICY_ADMIN_ROLE_OR_APPLICATION_MANAGER);
			c.AddPolicy(DemoPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_APP_PERM, DemoPolicies.POLICY_ADMIN_ROLE_OR_CREATE_APP_PERM);
			c.AddPolicy(DemoPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM, DemoPolicies.POLICY_ADMIN_ROLE_OR_MODIFY_APP_PERM);
			c.AddPolicy(DemoPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_APP_PERM, DemoPolicies.POLICY_ADMIN_ROLE_OR_DELETE_APP_PERM);
		});
	}
}
