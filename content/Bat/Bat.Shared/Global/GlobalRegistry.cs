using System.Security.Claims;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Identity;

namespace Bat.Shared.Global;

/// <summary>
/// Global registry to hold global/shared objects.
/// </summary>
public sealed class GlobalRegistry
{
	/// <summary>
	/// Set of all built-in claims (roles and permissions).
	/// </summary>
	public static readonly ISet<Claim> ALL_CLAIMS = new SortedSet<Claim>(ClaimComparer.INSTANCE);

	/// <summary>
	/// Convenience method to check if a claim exists/is valid.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool ClaimExists(string type, string value)
	{
		return ALL_CLAIMS.Any(c => c.Type.Equals(type, StringComparison.OrdinalIgnoreCase) && c.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Convenience method to check if a claim exists/is valid.
	/// </summary>
	/// <param name="claim"></param>
	/// <returns></returns>
	public static bool ClaimExists(Claim claim)
	{
		return ALL_CLAIMS.Contains(claim, ClaimEqualityComparer.INSTANCE);
	}

	/// <summary>
	/// Convenience method to check if a claim exists/is valid.
	/// </summary>
	/// <param name="claim"></param>
	/// <returns></returns>
	public static bool ClaimExists(IdentityClaim claim)
	{
		return ClaimExists(claim.Type, claim.Value);
	}

	/// <summary>
	/// Convenience method to check if a claim exists/is valid.
	/// </summary>
	/// <param name="claim"></param>
	/// <returns></returns>
	public static bool ClaimExists(IdentityRoleClaim<string> claim)
	{
		return ClaimExists(claim.ClaimType??string.Empty, claim.ClaimValue??string.Empty);
	}

	/// <summary>
	/// Convenience method to check if a claim exists/is valid.
	/// </summary>
	/// <param name="claim"></param>
	/// <returns></returns>
	public static bool ClaimExists(IdentityUserClaim<string> claim)
	{
		return ClaimExists(claim.ClaimType??string.Empty, claim.ClaimValue??string.Empty);
	}
}
