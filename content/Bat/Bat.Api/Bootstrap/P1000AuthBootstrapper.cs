using Bat.Api.Services;
using Bat.Shared.Api;
using Bat.Shared.Bootstrap;
using Bat.Shared.Identity;
using Bat.Shared.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Bat.Api.Bootstrap;

/// <summary>
/// Sample bootstrapper that initializes and register an <see cref="IAuthenticator"/> service.
/// </summary>
[Bootstrapper]
public class AuthBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder, IOptions<JwtOptions> jwtOptions)
	{
		// use JwtBearer authentication scheme
		appBuilder.Services.AddAuthentication()
			.AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.RequireHttpsMetadata = false;
				options.TokenValidationParameters = jwtOptions.Value.TokenValidationParameters;
			});

		// Customize the behavior of the authorization middleware.
		appBuilder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, SampleAuthorizationMiddlewareResultHandler>();

		// Configurate authorization policies
		appBuilder.Services.AddAuthorizationBuilder()
			.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_USER_PERM, BuiltinPolicies.POLICY_ADMIN_OR_CREATE_USER_PERM)
			.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_USER_PERM, BuiltinPolicies.POLICY_ADMIN_OR_MODIFY_USER_PERM)
			.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_USER_PERM, BuiltinPolicies.POLICY_ADMIN_OR_DELETE_USER_PERM)
			.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_APP_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_CREATE_APP_PERM)
			.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_MODIFY_APP_PERM)
			.AddPolicy(BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_APP_PERM, BuiltinPolicies.POLICY_ADMIN_ROLE_OR_DELETE_APP_PERM)
			;

		// setup IAuthenticator/IAuthenticatorAsync services
		appBuilder.Services.AddSingleton<SampleJwtAuthenticator>()
			// we want the lookup of both IAuthenticator and IAuthenticatorAsync to return the same SampleJwtAuthenticator instance
			.AddSingleton<IAuthenticator>(x => x.GetRequiredService<SampleJwtAuthenticator>())
			.AddSingleton<IAuthenticatorAsync>(x => x.GetRequiredService<SampleJwtAuthenticator>());
	}

	public static void DecorateApp(WebApplication app)
	{
		// activate authentication/authorization middleware
		app.UseAuthentication();
		app.UseAuthorization();
	}
}

/// <summary>
/// We want unauthorized API calls to return <see cref="ApiResp"/> instead of the default behavior.
/// </summary>
public class SampleAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
	private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();
	private readonly byte[] unauthorizedResult = JsonSerializer.SerializeToUtf8Bytes(new ApiResp
	{
		Status = StatusCodes.Status401Unauthorized,
		Message = "Unauthorized"
	});

	public async Task HandleAsync(
		RequestDelegate next,
		HttpContext context,
		AuthorizationPolicy policy,
		PolicyAuthorizationResult authorizeResult)
	{
		if (!authorizeResult.Succeeded)
		{
			if (authorizeResult.Challenged) await context.ChallengeAsync();
			else if (authorizeResult.Forbidden) await context.ForbidAsync();

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.BodyWriter.WriteAsync(unauthorizedResult);
			return;
		}

		// Fall back to the default implementation.
		await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
	}
}
