using Bat.Blazor.App.Services;
using Bat.Shared.Bootstrap;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Blazor.App.Bootstrap;

/// <summary>
/// Bootstrapper that registers identity/auth services.
/// </summary>
/// <remarks>
///		Bootstrappers in Blazor.App project are shared between for Blazor Server and Blazor WASM.
/// </remarks>
[Bootstrapper]
public class AuthBootstrapper
{
	public static void ConfigureServices(IServiceCollection services)
	{
		// Customize the behavior of the authorization middleware.
		//services.AddSingleton<IAuthorizationMiddlewareResultHandler, SampleAuthorizationMiddlewareResultHandler>();

		// set up authorization
		services.AddAuthorizationCore(c =>
		{
			// Configurate authorization policies
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_USER_MANAGER);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_USER_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_CREATE_USER_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_USER_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_MODIFY_USER_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_USER_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_DELETE_USER_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_ROLE_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_CREATE_ROLE_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_ROLE_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_MODIFY_ROLE_PERM);
			c.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_ROLE_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_DELETE_ROLE_PERM);
		});

		/* this has been done in Routes.razor with <CascadingAuthenticationState> tag */
		//services.AddCascadingAuthenticationState();

		// register the custom state provider
		services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

		//// https://stackoverflow.com/questions/70133682/hide-console-authorization-logs-in-blazor-webassembly
		//wasmAppBuilder.Logging.AddFilter("Microsoft.AspNetCore.Authorization.*", LogLevel.Warning);
	}
}
