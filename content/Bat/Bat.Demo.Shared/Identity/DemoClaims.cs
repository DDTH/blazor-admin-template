using System.Security.Claims;
using Bat.Shared.Identity;

namespace Bat.Demo.Shared.Identity;

public sealed class DemoClaims
{
	/// <summary>
	/// Claim to mark a user/role as an application manager.
	/// </summary>
	public static readonly Claim CLAIM_ROLE_APPLICATION_MANAGER = new($"{BuiltinClaims.CLAIM_PREFIX}{BuiltinClaims.ROLE_PREFIX}", "application-manager");

	/// <summary>
	/// Permission to create a new application.
	/// </summary>
	public static readonly Claim CLAIM_PERM_CREATE_APPLICATION = new($"{BuiltinClaims.CLAIM_PREFIX}{BuiltinClaims.PERMISSION_PREFIX}", "application-create");

	/// <summary>
	/// Permission to delete an application.
	/// </summary>
	public static readonly Claim CLAIM_PERM_DELETE_APPLICATION = new($"{BuiltinClaims.CLAIM_PREFIX}{BuiltinClaims.PERMISSION_PREFIX}", "application-delete");

	/// <summary>
	/// Permission to modify an application.
	/// </summary>
	public static readonly Claim CLAIM_PERM_MODIFY_APPLICATION = new($"{BuiltinClaims.CLAIM_PREFIX}{BuiltinClaims.PERMISSION_PREFIX}", "application-modify");

	public static readonly IEnumerable<Claim> ALL_CLAIMS = [
		CLAIM_ROLE_APPLICATION_MANAGER,
		CLAIM_PERM_CREATE_APPLICATION,
		CLAIM_PERM_DELETE_APPLICATION,
		CLAIM_PERM_MODIFY_APPLICATION,
	];
}
