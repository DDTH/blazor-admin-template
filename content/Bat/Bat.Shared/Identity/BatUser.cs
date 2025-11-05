using Microsoft.AspNetCore.Identity;

namespace Bat.Shared.Identity;

public sealed class BatUser : IdentityUser
{
	public IEnumerable<BatRole>? Roles { get; set; } = default!;
	public IEnumerable<IdentityUserClaim<string>>? Claims { get; set; } = default!;

	public string? GivenName { get; set; } = default!;
	public string? FamilyName { get; set; } = default!;

	/// <summary>
	/// Touches the entity, updating the <see cref="ConcurrencyStamp"/> property.
	/// </summary>
	public void Touch() => ConcurrencyStamp = Guid.NewGuid().ToString();

	public override bool Equals(object? obj) => obj is BatUser other
		&& (ReferenceEquals(this, other) || Id.Equals(other.Id, StringComparison.Ordinal));

	public override int GetHashCode() => Id.GetHashCode(StringComparison.Ordinal);
}
