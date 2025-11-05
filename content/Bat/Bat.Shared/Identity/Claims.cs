using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Bat.Shared.Identity;

/// <summary>
/// Convenience struct to represent a claim in API requests/responses.
/// </summary>
public struct IdentityClaim
{
	/// <summary>
	/// Convenience method to create a new instance of <see cref="IdentityClaim"/>.
	/// </summary>
	/// <param name="typeAndValue">the claim type and value in format claim-type:claim-value</param>
	/// <returns>null is returned in the input is invalid</returns>
	public static IdentityClaim? CreateFrom(string typeAndValue)
	{
		var parts = typeAndValue.Split(':');
		return parts.Length != 2 ? null : new IdentityClaim{ Type = parts[0], Value = parts[1] };
	}

	[JsonPropertyName("type")]
	public string Type { get; set; }

	[JsonPropertyName("value")]
	public string Value { get; set; }
}

public sealed class BuiltinClaims
{
	public const string CLAIM_PREFIX = "#";
	public const string ROLE_PREFIX = "role";
	public const string PERMISSION_PREFIX = "perm";

	/// <summary>
	/// Claim to mark a user/role as a global administrator.
	/// </summary>
	public static readonly Claim CLAIM_ROLE_GLOBAL_ADMIN = new($"{CLAIM_PREFIX}{ROLE_PREFIX}", "global-admin");

	/// <summary>
	/// Claim to mark a user/role as an user manager.
	/// </summary>
	public static readonly Claim CLAIM_ROLE_USER_MANAGER = new($"{CLAIM_PREFIX}{ROLE_PREFIX}", "user-manager");

	/// <summary>
	/// Permission to create a new user account.
	/// </summary>
	public static readonly Claim CLAIM_PERM_CREATE_USER = new($"{CLAIM_PREFIX}{PERMISSION_PREFIX}", "user-create");

	/// <summary>
	/// Permission to delete a user account.
	/// </summary>
	public static readonly Claim CLAIM_PERM_DELETE_USER = new($"{CLAIM_PREFIX}{PERMISSION_PREFIX}", "user-delete");

	/// <summary>
	/// Permission to modify a user account.
	/// </summary>
	public static readonly Claim CLAIM_PERM_MODIFY_USER = new($"{CLAIM_PREFIX}{PERMISSION_PREFIX}", "user-modify");

	/// <summary>
	/// Permission to create a new role.
	/// </summary>
	public static readonly Claim CLAIM_PERM_CREATE_ROLE = new($"{CLAIM_PREFIX}{PERMISSION_PREFIX}", "role-create");

	/// <summary>
	/// Permission to delete a role.
	/// </summary>
	public static readonly Claim CLAIM_PERM_DELETE_ROLE = new($"{CLAIM_PREFIX}{PERMISSION_PREFIX}", "role-delete");

	/// <summary>
	/// Permission to modify a role.
	/// </summary>
	public static readonly Claim CLAIM_PERM_MODIFY_ROLE = new($"{CLAIM_PREFIX}{PERMISSION_PREFIX}", "role-modify");

	public static readonly IEnumerable<Claim> ALL_CLAIMS = [
		CLAIM_ROLE_GLOBAL_ADMIN,
		CLAIM_ROLE_USER_MANAGER,
		CLAIM_PERM_CREATE_USER,
		CLAIM_PERM_DELETE_USER,
		CLAIM_PERM_MODIFY_USER,
		CLAIM_PERM_CREATE_ROLE,
		CLAIM_PERM_DELETE_ROLE,
		CLAIM_PERM_MODIFY_ROLE,
	];
}

public sealed class ClaimEqualityComparer : IEqualityComparer<Claim>
{
	public static readonly ClaimEqualityComparer INSTANCE = new();

	public bool Equals(Claim? x, Claim? y)
	{
		if (x == null && y == null) return true;
		if (x == null || y == null) return false;
		return x.Type.Equals(y.Type, StringComparison.OrdinalIgnoreCase) && x.Value.Equals(y.Value, StringComparison.OrdinalIgnoreCase);
	}

	public int GetHashCode(Claim obj)
	{
		return HashCode.Combine(obj.Type, obj.Value);
	}
}

public sealed class ClaimComparer : IComparer<Claim>
{
	public static readonly ClaimComparer INSTANCE = new();

	public int Compare(Claim? x, Claim? y)
	{
		if (x == null && y == null) return 0;
		if (x == null) return -1;
		if (y == null) return 1;

		var typeComparison = string.Compare(x.Type, y.Type, StringComparison.OrdinalIgnoreCase);
		if (typeComparison != 0) return typeComparison;
		return string.Compare(x.Value, y.Value, StringComparison.OrdinalIgnoreCase);
	}
}
