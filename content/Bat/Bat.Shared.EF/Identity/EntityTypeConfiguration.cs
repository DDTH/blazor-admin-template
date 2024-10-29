using Bat.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bat.Shared.EF.Identity;

/* Demostration of how to use IEntityTypeConfiguration to customize table and column names */

sealed class RoleEntityTypeConfiguration : IEntityTypeConfiguration<BatRole>
{
	public void Configure(EntityTypeBuilder<BatRole> builder)
	{
		builder.ToTable($"{Globals.TABLE_PREFIX}roles");
		builder.Property(t => t.Id).HasColumnName("role_id");
		builder.Property(t => t.Name).HasColumnName("role_name");
		builder.Property(t => t.NormalizedName).HasColumnName("normalized_name");
		builder.Property(t => t.ConcurrencyStamp).HasColumnName("concurrency_stamp").IsConcurrencyToken();
		builder.Property(t => t.Description).HasColumnName("role_desc");

		builder.Ignore(t => t.Claims);
	}
}

sealed class IdentityRoleClaimEntityTypeConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<string>>
{
	public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
	{
		builder.ToTable($"{Globals.TABLE_PREFIX}role_claims");
		//builder.Property(t => t.Id).HasColumnName("rc_id");
		builder.Property(t => t.RoleId).HasColumnName("role_id");
		builder.Property(t => t.ClaimType).HasColumnName("claim_type");
		builder.Property(t => t.ClaimValue).HasColumnName("claim_value");

		// {RoleId, ClaimType, ClaimValue} should be unique
		builder.HasKey(t => new { t.RoleId, t.ClaimType, t.ClaimValue });
		//builder.HasIndex(t => new { t.RoleId, t.ClaimType, t.ClaimValue }).IsUnique();

		builder.
			Ignore(t => t.Id)
			;
	}
}

sealed class IdentityUserEntityTypeConfiguration : IEntityTypeConfiguration<BatUser>
{
	public void Configure(EntityTypeBuilder<BatUser> builder)
	{
		builder.ToTable($"{Globals.TABLE_PREFIX}users");
		builder.Property(t => t.Id).HasColumnName("uid");
		builder.Property(t => t.UserName).HasColumnName("uname");
		builder.Property(t => t.NormalizedUserName).HasColumnName("normalized_name");
		builder.Property(t => t.Email).HasColumnName("uemail");
		builder.Property(t => t.NormalizedEmail).HasColumnName("normalized_email");
		//builder.Property(t => t.EmailConfirmed).HasColumnName("email_confirmed");
		builder.Property(t => t.PasswordHash).HasColumnName("password_hash");
		builder.Property(t => t.SecurityStamp).HasColumnName("security_stamp");
		builder.Property(t => t.ConcurrencyStamp).HasColumnName("concurrency_stamp").IsConcurrencyToken();
		//builder.Property(t => t.PhoneNumber).HasColumnName("phone_number");
		//builder.Property(t => t.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
		//builder.Property(t => t.TwoFactorEnabled).HasColumnName("two_factor_enabled");
		//builder.Property(t => t.LockoutEnd).HasColumnName("lockout_end");
		//builder.Property(t => t.LockoutEnabled).HasColumnName("lockout_enabled");
		//builder.Property(t => t.AccessFailedCount).HasColumnName("access_failed_count");
		builder.Property(t => t.GivenName).HasColumnName("given_name");
		builder.Property(t => t.FamilyName).HasColumnName("family_name");

		// username and email should be unique
		builder.HasIndex(t => t.NormalizedUserName).IsUnique();
		builder.HasIndex(t => t.NormalizedEmail).IsUnique();

		// ignore these properties, we are not using them
		builder
			.Ignore(t => t.EmailConfirmed)
			.Ignore(t => t.PhoneNumber)
			.Ignore(t => t.PhoneNumberConfirmed)
			.Ignore(t => t.TwoFactorEnabled)
			.Ignore(t => t.LockoutEnd)
			.Ignore(t => t.LockoutEnabled)
			.Ignore(t => t.AccessFailedCount)
			.Ignore(t => t.Roles)
			.Ignore(t => t.Claims)
			;
	}
}

sealed class IdentityUserClaimEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserClaim<string>>
{
	public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
	{
		builder.ToTable($"{Globals.TABLE_PREFIX}user_claims");
		//builder.Property(t => t.Id).HasColumnName("uc_id");
		builder.Property(t => t.UserId).HasColumnName("user_id");
		builder.Property(t => t.ClaimType).HasColumnName("claim_type");
		builder.Property(t => t.ClaimValue).HasColumnName("claim_value");

		// {UserId, ClaimType, ClaimValue} should be unique
		builder.HasKey(t => new { t.UserId, t.ClaimType, t.ClaimValue });
		//builder.HasIndex(t => new { t.UserId, t.ClaimType, t.ClaimValue }).IsUnique();

		builder.
			Ignore(t => t.Id)
			;
	}
}

sealed class IdentityUserLoginEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserLogin<string>>
{
	public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
	{
		builder.ToTable($"{Globals.TABLE_PREFIX}user_logins");
		builder.Property(t => t.LoginProvider).HasColumnName("login_provider");
		builder.Property(t => t.ProviderKey).HasColumnName("provider_key");
		builder.Property(t => t.ProviderDisplayName).HasColumnName("provider_display_name");
		builder.Property(t => t.UserId).HasColumnName("user_id");
	}
}

sealed class IdentityUserRoleEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
	public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
	{
		builder.ToTable($"{Globals.TABLE_PREFIX}user_roles");
		builder.Property(t => t.UserId).HasColumnName("user_id");
		builder.Property(t => t.RoleId).HasColumnName("role_id");
	}
}

sealed class IdentityUserTokenEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserToken<string>>
{
	public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
	{
		builder.ToTable($"{Globals.TABLE_PREFIX}user_tokens");
		builder.Property(t => t.UserId).HasColumnName("user_id");
		builder.Property(t => t.LoginProvider).HasColumnName("login_provider");
		builder.Property(t => t.Name).HasColumnName("token_name");
		builder.Property(t => t.Value).HasColumnName("token_value");
	}
}
