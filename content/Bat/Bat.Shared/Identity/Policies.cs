using Microsoft.AspNetCore.Authorization;

namespace Bat.Shared.Identity;

public sealed class BuiltinPolicies
{
	public const string POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER = "AdminRoleOrUserManager";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_USER_MANAGER = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			Console.WriteLine($"{POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER} - User claims:");
			context.User.Claims.ToList().ForEach(c => Console.WriteLine($"User claim: {c.Type} = {c.Value}"));

			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasUserManagerRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_USER_MANAGER.Type, BuiltinClaims.CLAIM_ROLE_USER_MANAGER.Value);
			return hasAdminRole || hasUserManagerRole;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_APPLICATION_MANAGER = "AdminRoleOrApplicationManager";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_APPLICATION_MANAGER = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasApplicationManagerRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_APPLICATION_MANAGER.Type, BuiltinClaims.CLAIM_ROLE_APPLICATION_MANAGER.Value);
			return hasAdminRole || hasApplicationManagerRole;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_CREATE_USER_PERM = "AdminRoleOrCreateUserPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_CREATE_USER_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasCreateUserPerm = context.User.HasClaim(BuiltinClaims.CLAIM_PERM_CREATE_USER.Type, BuiltinClaims.CLAIM_PERM_CREATE_USER.Value);
			return hasAdminRole || hasCreateUserPerm;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_MODIFY_USER_PERM = "AdminRoleOrModifyUserPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_MODIFY_USER_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasModifyUserPerm = context.User.HasClaim(BuiltinClaims.CLAIM_PERM_MODIFY_USER.Type, BuiltinClaims.CLAIM_PERM_MODIFY_USER.Value);
			return hasAdminRole || hasModifyUserPerm;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_DELETE_USER_PERM = "AdminRoleOrDeleteUserPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_DELETE_USER_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasDeleteUserPerm = context.User.HasClaim(BuiltinClaims.CLAIM_PERM_DELETE_USER.Type, BuiltinClaims.CLAIM_PERM_DELETE_USER.Value);
			return hasAdminRole || hasDeleteUserPerm;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_CREATE_APP_PERM = "AdminRoleOrCreateAppPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_CREATE_APP_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasCreateAppPerm = context.User.HasClaim(BuiltinClaims.CLAIM_PERM_CREATE_APPLICATION.Type, BuiltinClaims.CLAIM_PERM_CREATE_APPLICATION.Value);
			return hasAdminRole || hasCreateAppPerm;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM = "AdminRoleOrModifyAppPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_MODIFY_APP_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasModifyAppPerm = context.User.HasClaim(BuiltinClaims.CLAIM_PERM_MODIFY_APPLICATION.Type, BuiltinClaims.CLAIM_PERM_MODIFY_APPLICATION.Value);
			return hasAdminRole || hasModifyAppPerm;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_DELETE_APP_PERM = "AdminRoleOrDeleteAppPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_DELETE_APP_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasDeleteAppPerm = context.User.HasClaim(BuiltinClaims.CLAIM_PERM_DELETE_APPLICATION.Type, BuiltinClaims.CLAIM_PERM_DELETE_APPLICATION.Value);
			return hasAdminRole || hasDeleteAppPerm;
		})
		.Build();
}
