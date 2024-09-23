using Microsoft.AspNetCore.Identity;

namespace Bat.Shared.Identity;

public partial class BatRole
{
	public const string ROLE_NAME_GLOBAL_ADMIN = "Global Admin";
	public const string ROLE_NAME_ACCOUNT_ADMIN = "Account Admin";
	public const string ROLE_NAME_APPLICATION_ADMIN = "Application Admin";
	public static readonly IEnumerable<string> ALL_BUILTIN_ROLE_NAMES_NORMALIZED = [
		ROLE_NAME_GLOBAL_ADMIN.ToUpper(),
		ROLE_NAME_ACCOUNT_ADMIN.ToUpper(),
		ROLE_NAME_APPLICATION_ADMIN.ToUpper()
	];

	public const string ROLE_ID_GLOBAL_ADMIN = "global-admin";
	public const string ROLE_ID_ACCOUNT_ADMIN = "account-admin";
	public const string ROLE_ID_APPLICATION_ADMIN = "application-admin";
	public static readonly IEnumerable<string> ALL_BUILTIN_ROLE_IDS = [
		ROLE_ID_GLOBAL_ADMIN,
		ROLE_ID_ACCOUNT_ADMIN,
		ROLE_ID_APPLICATION_ADMIN
	];

	public static readonly BatRole GLOBAL_ADMIN = new()
	{
		Id = ROLE_ID_GLOBAL_ADMIN,
		Name = ROLE_NAME_GLOBAL_ADMIN,
		Claims =
		[
			new IdentityRoleClaim<string>()
			{
				RoleId=ROLE_ID_GLOBAL_ADMIN,
				ClaimType=BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Type,
				ClaimValue=BuiltinClaims.CLAIM_ROLE_GLOBAL_ADMIN.Value,
			}
		]
	};
	public static readonly BatRole ACCOUNT_ADMIN = new()
	{
		Id = ROLE_ID_ACCOUNT_ADMIN,
		Name = ROLE_NAME_ACCOUNT_ADMIN,
		Claims =
		[
			new IdentityRoleClaim<string>()
			{
				RoleId=ROLE_ID_ACCOUNT_ADMIN,
				ClaimType=BuiltinClaims.CLAIM_PERM_CREATE_USER.Type,
				ClaimValue=BuiltinClaims.CLAIM_PERM_CREATE_USER.Value,
			},
			new IdentityRoleClaim<string>()
			{
				RoleId=ROLE_ID_ACCOUNT_ADMIN,
				ClaimType=BuiltinClaims.CLAIM_PERM_DELETE_USER.Type,
				ClaimValue=BuiltinClaims.CLAIM_PERM_DELETE_USER.Value,
			},
			new IdentityRoleClaim<string>()
			{
				RoleId=ROLE_ID_ACCOUNT_ADMIN,
				ClaimType=BuiltinClaims.CLAIM_PERM_MODIFY_USER.Type,
				ClaimValue=BuiltinClaims.CLAIM_PERM_MODIFY_USER.Value,
			}
		]
	};
	public static readonly BatRole APPLICATION_ADMIN = new()
	{
		Id = ROLE_ID_APPLICATION_ADMIN,
		Name = ROLE_NAME_APPLICATION_ADMIN,
		Claims =
		[
			new IdentityRoleClaim<string>()
			{
				RoleId=ROLE_ID_APPLICATION_ADMIN,
				ClaimType=BuiltinClaims.CLAIM_PERM_CREATE_APPLICATION.Type,
				ClaimValue=BuiltinClaims.CLAIM_PERM_CREATE_APPLICATION.Value,
			},
			new IdentityRoleClaim<string>()
			{
				RoleId=ROLE_ID_APPLICATION_ADMIN,
				ClaimType=BuiltinClaims.CLAIM_PERM_DELETE_APPLICATION.Type,
				ClaimValue=BuiltinClaims.CLAIM_PERM_DELETE_APPLICATION.Value,
			},
			new IdentityRoleClaim<string>()
			{
				RoleId=ROLE_ID_APPLICATION_ADMIN,
				ClaimType=BuiltinClaims.CLAIM_PERM_MODIFY_APPLICATION.Type,
				ClaimValue=BuiltinClaims.CLAIM_PERM_MODIFY_APPLICATION.Value,
			}
		]
	};
	public static readonly IEnumerable<BatRole> ALL_BUILTIN_ROLES = [
		GLOBAL_ADMIN,
		ACCOUNT_ADMIN,
		APPLICATION_ADMIN
	];
}
