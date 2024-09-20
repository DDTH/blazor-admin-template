using Microsoft.AspNetCore.Identity;

namespace Bat.Shared.Identity;

public sealed class BatRole : IdentityRole
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
	};
	public static readonly BatRole ACCOUNT_ADMIN = new()
	{
		Id = ROLE_ID_ACCOUNT_ADMIN,
		Name = ROLE_NAME_ACCOUNT_ADMIN,
	};
	public static readonly BatRole APPLICATION_ADMIN = new()
	{
		Id = ROLE_ID_APPLICATION_ADMIN,
		Name = ROLE_NAME_APPLICATION_ADMIN,
	};
	public static readonly IEnumerable<BatRole> ALL_BUILTIN_ROLES = [
		GLOBAL_ADMIN,
		ACCOUNT_ADMIN,
		APPLICATION_ADMIN
		];

	/// <summary>
	/// Touches the entity, updating the <see cref="ConcurrencyStamp"/> property.
	/// </summary>
	public void Touch() => ConcurrencyStamp = Guid.NewGuid().ToString();

	public override bool Equals(object? obj) => obj is BatRole other
		&& (ReferenceEquals(this, other) || Id.Equals(other.Id, Globals.StringComparison));

	public override int GetHashCode() => Id.GetHashCode(Globals.StringComparison);
}
