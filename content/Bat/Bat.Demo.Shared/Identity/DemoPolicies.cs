using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Bat.Demo.Shared.Identity;

public sealed class DemoPolicies
{
	public const string POLICY_NAME_ADMIN_ROLE_OR_APPLICATION_MANAGER = "AdminRoleOrApplicationManager";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_APPLICATION_MANAGER = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasApplicationManagerRole = context.User.HasClaim(DemoClaims.CLAIM_ROLE_APPLICATION_MANAGER.Type, DemoClaims.CLAIM_ROLE_APPLICATION_MANAGER.Value);
			return hasAdminRole || hasApplicationManagerRole;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_CREATE_APP_PERM = "AdminRoleOrCreateAppPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_CREATE_APP_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasCreateAppPerm = context.User.HasClaim(DemoClaims.CLAIM_PERM_CREATE_APPLICATION.Type, DemoClaims.CLAIM_PERM_CREATE_APPLICATION.Value);
			return hasAdminRole || hasCreateAppPerm;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_MODIFY_APP_PERM = "AdminRoleOrModifyAppPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_MODIFY_APP_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasModifyAppPerm = context.User.HasClaim(DemoClaims.CLAIM_PERM_MODIFY_APPLICATION.Type, DemoClaims.CLAIM_PERM_MODIFY_APPLICATION.Value);
			return hasAdminRole || hasModifyAppPerm;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_DELETE_APP_PERM = "AdminRoleOrDeleteAppPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_DELETE_APP_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasDeleteAppPerm = context.User.HasClaim(DemoClaims.CLAIM_PERM_DELETE_APPLICATION.Type, DemoClaims.CLAIM_PERM_DELETE_APPLICATION.Value);
			return hasAdminRole || hasDeleteAppPerm;
		})
		.Build();
}
