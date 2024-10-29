using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Bat.Shared.Identity;

/// <summary>
/// Convenience struct to represent a claim in API requests/responses.
/// </summary>
public struct IdentityClaim
{
	[JsonPropertyName("type")]
	public string Type { get; set; }

	[JsonPropertyName("value")]
	public string Value { get; set; }
}

public sealed class BuiltinClaims
{
	private const string CLAIM_PREFIX = "#";

	/// <summary>
	/// Claim to mark a user/role as a global administrator.
	/// </summary>
	public static readonly Claim CLAIM_ROLE_GLOBAL_ADMIN = new($"{CLAIM_PREFIX}role", "global-admin");

	/// <summary>
	/// Claim to mark a user/role as an user manager.
	/// </summary>
	public static readonly Claim CLAIM_ROLE_USER_MANAGER = new($"{CLAIM_PREFIX}role", "user-manager");

	/// <summary>
	/// Claim to mark a user/role as an application manager.
	/// </summary>
	public static readonly Claim CLAIM_ROLE_APPLICATION_MANAGER = new($"{CLAIM_PREFIX}role", "application-manager");

	/// <summary>
	/// Permission to create a new application.
	/// </summary>
	public static readonly Claim CLAIM_PERM_CREATE_APPLICATION = new($"{CLAIM_PREFIX}perm", "create-application");

	/// <summary>
	/// Permission to delete an application.
	/// </summary>
	public static readonly Claim CLAIM_PERM_DELETE_APPLICATION = new($"{CLAIM_PREFIX}perm", "delete-application");

	/// <summary>
	/// Permission to modify an application.
	/// </summary>
	public static readonly Claim CLAIM_PERM_MODIFY_APPLICATION = new($"{CLAIM_PREFIX}perm", "modify-application");

	/// <summary>
	/// Permission to create a new user account.
	/// </summary>
	public static readonly Claim CLAIM_PERM_CREATE_USER = new($"{CLAIM_PREFIX}perm", "create-user");

	/// <summary>
	/// Permission to delete a user account.
	/// </summary>
	public static readonly Claim CLAIM_PERM_DELETE_USER = new($"{CLAIM_PREFIX}perm", "delete-user");

	/// <summary>
	/// Permission to modify a user account.
	/// </summary>
	public static readonly Claim CLAIM_PERM_MODIFY_USER = new($"{CLAIM_PREFIX}perm", "modify-user");

	/// <summary>
	/// Permission to create a new role.
	/// </summary>
	public static readonly Claim CLAIM_PERM_CREATE_ROLE = new($"{CLAIM_PREFIX}perm", "create-role");

	/// <summary>
	/// Permission to delete a role.
	/// </summary>
	public static readonly Claim CLAIM_PERM_DELETE_ROLE = new($"{CLAIM_PREFIX}perm", "delete-role");

	/// <summary>
	/// Permission to modify a role.
	/// </summary>
	public static readonly Claim CLAIM_PERM_MODIFY_ROLE = new($"{CLAIM_PREFIX}perm", "modify-role");

	private static readonly IEnumerable<Claim> ALL_CLAIMS_ROLES = new Claim[]
	{
		CLAIM_ROLE_GLOBAL_ADMIN,
		CLAIM_ROLE_APPLICATION_MANAGER,
		CLAIM_ROLE_USER_MANAGER,
	}.OrderBy(c => c.Type).ThenBy(c => c.Value);

	private static readonly IEnumerable<Claim> ALL_CLAIMS_PERMS = new Claim[]
	{
		CLAIM_PERM_CREATE_APPLICATION,
		CLAIM_PERM_DELETE_APPLICATION,
		CLAIM_PERM_MODIFY_APPLICATION,
		CLAIM_PERM_CREATE_USER,
		CLAIM_PERM_DELETE_USER,
		CLAIM_PERM_MODIFY_USER,
	}.OrderBy(c => c.Type).ThenBy(c => c.Value);

	public static readonly IEnumerable<Claim> ALL_CLAIMS = ALL_CLAIMS_ROLES.Concat(ALL_CLAIMS_PERMS);
}

public class ClaimEqualityComparer : IEqualityComparer<Claim>
{
	public static readonly ClaimEqualityComparer INSTANCE = new();

	public bool Equals(Claim? x, Claim? y)
	{
		if (x == null && y == null) return true;
		if (x == null || y == null) return false;
		return x.Type.Equals(y.Type, Globals.StringComparison) && x.Value.Equals(y.Value, Globals.StringComparison);
	}

	public int GetHashCode(Claim obj)
	{
		return HashCode.Combine(obj.Type, obj.Value);
	}
}
