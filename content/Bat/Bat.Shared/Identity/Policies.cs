using Microsoft.AspNetCore.Authorization;

namespace Bat.Shared.Identity;

public sealed class BuiltinPolicies
{
	public const string POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER = "AdminRoleOrUserManager";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_USER_MANAGER = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasUserManagerRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_USER_MANAGER.Type, BuiltinClaims.CLAIM_ROLE_USER_MANAGER.Value);
			return hasAdminRole || hasUserManagerRole;
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

	public const string POLICY_NAME_ADMIN_ROLE_OR_CREATE_ROLE_PERM = "AdminRoleOrCreateRolePermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_CREATE_ROLE_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasCreateRolePerm = context.User.HasClaim(BuiltinClaims.CLAIM_PERM_CREATE_ROLE.Type, BuiltinClaims.CLAIM_PERM_CREATE_ROLE.Value);
			return hasAdminRole || hasCreateRolePerm;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_MODIFY_ROLE_PERM = "AdminRoleOrModifyRolePermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_MODIFY_ROLE_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasModifyRolePerm = context.User.HasClaim(BuiltinClaims.CLAIM_PERM_MODIFY_ROLE.Type, BuiltinClaims.CLAIM_PERM_MODIFY_ROLE.Value);
			return hasAdminRole || hasModifyRolePerm;
		})
		.Build();

	public const string POLICY_NAME_ADMIN_ROLE_OR_DELETE_ROLE_PERM = "AdminRoleOrDeleteRolePermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_ROLE_OR_DELETE_ROLE_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.HasClaim(BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type, BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value);
			var hasDeleteRolePerm = context.User.HasClaim(BuiltinClaims.CLAIM_PERM_DELETE_ROLE.Type, BuiltinClaims.CLAIM_PERM_DELETE_ROLE.Value);
			return hasAdminRole || hasDeleteRolePerm;
		})
		.Build();
}
