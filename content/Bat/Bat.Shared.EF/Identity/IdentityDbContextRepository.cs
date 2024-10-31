using Bat.Shared.Cache;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace Bat.Shared.EF.Identity;

public sealed class IdentityDbContextRepository : IdentityDbContext<BatUser, BatRole, string>, IIdentityRepository
{
	private readonly ICacheFacade<IIdentityRepository>? cache;

	public IdentityDbContextRepository(
		DbContextOptions<IdentityDbContextRepository> options,
		ICacheFacade<IIdentityRepository>? cache = default) : base(options)
	{
		this.cache = cache;
	}

	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//{
	//	base.OnConfiguring(optionsBuilder);
	//}

	private void ChangeTracker_DetectedAllChanges(object? sender, Microsoft.EntityFrameworkCore.ChangeTracking.DetectedChangesEventArgs e) => throw new NotImplementedException();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		new RoleEntityTypeConfiguration().Configure(modelBuilder.Entity<BatRole>());
		new IdentityRoleClaimEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityRoleClaim<string>>());
		new IdentityUserEntityTypeConfiguration().Configure(modelBuilder.Entity<BatUser>());
		new IdentityUserClaimEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityUserClaim<string>>());
		//new IdentityUserLoginEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityUserLogin<string>>());
		new IdentityUserRoleEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityUserRole<string>>());
		//new IdentityUserTokenEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityUserToken<string>>());

		modelBuilder.Ignore<IdentityUserLogin<string>>();
		modelBuilder.Ignore<IdentityUserToken<string>>();
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> CreateAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		var entry = Users.Add(user);
		entry.Entity.SecurityStamp = Guid.NewGuid().ToString("N");
		entry.Entity.Touch();
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? IdentityResult.Success : IIdentityRepository.NoChangesSaved;
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> CreateIfNotExistsAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		if (await GetUserByIDAsync(user.Id, cancellationToken: cancellationToken) != null) return IdentityResult.Success;
		if (await GetUserByUserNameAsync(user.UserName??string.Empty, cancellationToken: cancellationToken) != null) return IdentityResult.Success;
		if (await GetUserByEmailAsync(user.Email??string.Empty, cancellationToken: cancellationToken) != null) return IdentityResult.Success;
		return await CreateAsync(user, cancellationToken);
	}

	private async ValueTask<BatUser?> PostFetchUser(BatUser? user, UserFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		if (user is null || options is null || cancellationToken.IsCancellationRequested) return user;
		user.Roles = options.IncludeRoles ? await GetRolesAsync(
			user,
			roleFetchOptions: options.IncludeRoleClaims ? new RoleFetchOptions { IncludeClaims = true } : null,
			cancellationToken: cancellationToken) : null;
		user.Claims = options.IncludeClaims ? await GetClaimsAsync(user, cancellationToken) : null;
		return user;
	}

	/// <inheritdoc/>
	public async ValueTask<BatUser?> GetUserByIDAsync(string userId, UserFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		var user = await Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
		return await PostFetchUser(user, options, cancellationToken);
	}

	/// <inheritdoc/>
	public async ValueTask<BatUser?> GetUserByEmailAsync(string email, UserFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		var user = await Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
		return await PostFetchUser(user, options, cancellationToken);
	}

	/// <inheritdoc/>
	public async ValueTask<BatUser?> GetUserByUserNameAsync(string userName, UserFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		var user = await Users.FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
		return await PostFetchUser(user, options, cancellationToken);
	}

	/// <inheritdoc/>
	public IAsyncEnumerable<BatUser> AllUsersAsync()
	{
		return Users.AsAsyncEnumerable();
	}

	/// <inheritdoc/>
	public async ValueTask<BatUser?> UpdateAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		var entry = Users.Update(user);
		entry.Entity.Touch();
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? entry.Entity : null;
	}

	/// <inheritdoc/>
	public async ValueTask<BatUser?> UpdateSecurityStampAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		user.SecurityStamp = Guid.NewGuid().ToString("N");
		return await UpdateAsync(user, cancellationToken);
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> DeleteAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		Users.Remove(user);
		await SaveChangesAsync(cancellationToken);
		return IdentityResult.Success;
	}

	/// <inheritdoc/>
	public async ValueTask<IEnumerable<BatRole>> GetRolesAsync(BatUser user, RoleFetchOptions? roleFetchOptions = default, CancellationToken cancellationToken = default)
	{
		var roles = await UserRoles
			.Where(ur => ur.UserId == user.Id)
			.Join(Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
			.ToListAsync(cancellationToken) ?? [];
		foreach (var role in roles)
		{
			await PostFetchRole(role, roleFetchOptions, cancellationToken);
		}
		return roles;
	}

	/// <inheritdoc/>
	public async ValueTask<IEnumerable<IdentityUserClaim<string>>> GetClaimsAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		return await UserClaims
			.Where(uc => uc.UserId == user.Id)
			.ToListAsync(cancellationToken) ?? [];
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> AddToRolesAsync(BatUser user, IEnumerable<BatRole> roles, CancellationToken cancellationToken = default)
	{
		var rolesList = roles is not null ? roles.ToList() : []; // Convert to list to avoid multiple enumerations
		if (rolesList.Count == 0) return IdentityResult.Success;
		foreach (var role in rolesList)
		{
			UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
		}
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? IdentityResult.Success : IIdentityRepository.NoChangesSaved;
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> AddToRolesAsync(BatUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
	{
		var roles = new List<BatRole>();
		foreach (var roleName in roleNames)
		{
			var role = await GetRoleByNameAsync(roleName, cancellationToken: cancellationToken);
			if (role is null)
				return IdentityResult.Failed(new IdentityError()
				{
					Code = "RoleNotFound",
					Description = $"Role '{roleName}' does not exist."
				});
			roles.Add(role);
		}
		return await AddToRolesAsync(user, roles, cancellationToken: cancellationToken);
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> AddToRoleIfNotExistsAsync(BatUser user, BatRole role, CancellationToken cancellationToken = default)
	{
		var existing = await UserRoles.FirstOrDefaultAsync(
			ur => ur.UserId == user.Id && ur.RoleId == role.Id,
			cancellationToken);
		return existing is not null ? IdentityResult.Success : await AddToRolesAsync(user, [role], cancellationToken);
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> AddToRoleIfNotExistsAsync(BatUser user, string roleName, CancellationToken cancellationToken = default)
	{
		var role = await GetRoleByNameAsync(roleName, cancellationToken: cancellationToken);
		return role != null
			? await AddToRoleIfNotExistsAsync(user, role, cancellationToken)
			: IdentityResult.Failed(new IdentityError()
			{
				Code = "RoleNotFound",
				Description = $"Role '{roleName}' does not exist."
			});
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> RemoveFromRolesAsync(BatUser user, IEnumerable<BatRole> roles, CancellationToken cancellationToken = default)
	{
		var rolesList = roles is not null ? roles.ToList() : []; // Convert to list to avoid multiple enumerations
		if (rolesList.Count == 0) return IdentityResult.Success;
		rolesList.ForEach(role => UserRoles.Remove(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id }));
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? IdentityResult.Success : IIdentityRepository.NoChangesSaved;
	}

	public async ValueTask<IdentityResult> RemoveFromRolesAsync(BatUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
	{
		var roles = new List<BatRole>();
		foreach (var roleName in roleNames)
		{
			var role = await GetRoleByNameAsync(roleName, cancellationToken: cancellationToken);
			if (role is null)
				return IdentityResult.Failed(new IdentityError()
				{
					Code = "RoleNotFound",
					Description = $"Role '{roleName}' does not exist."
				});
			roles.Add(role);
		}
		return await RemoveFromRolesAsync(user, roles, cancellationToken: cancellationToken);
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> AddClaimsAsync(BatUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
	{
		UserClaims.AddRange(claims.Select(c => new IdentityUserClaim<string>
		{
			UserId = user.Id,
			ClaimType = c.Type,
			ClaimValue = c.Value
		}));
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? IdentityResult.Success : IIdentityRepository.NoChangesSaved;
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> AddClaimIfNotExistsAsync(BatUser user, Claim claim, CancellationToken cancellationToken = default)
	{
		var existing = await UserClaims.FirstOrDefaultAsync(
			rc => rc.UserId == user.Id && rc.ClaimType == claim.Type && rc.ClaimValue == claim.Value,
			cancellationToken);
		return existing is not null ? IdentityResult.Success : await AddClaimsAsync(user, [claim], cancellationToken);
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> RemoveClaimsAsync(BatUser user, IEnumerable<IdentityUserClaim<string>> claims, CancellationToken cancellationToken = default)
	{
		var claimsList = claims.ToList(); // Convert to list to avoid multiple enumerations
		var userClaims = user.Claims ?? await GetClaimsAsync(user, cancellationToken);
		foreach (var uc in userClaims)
		{
			if (claimsList.Any(c => c.UserId == user.Id && c.ClaimType == uc.ClaimType && c.ClaimValue == uc.ClaimValue))
			{
				UserClaims.Remove(uc);
			}
		}

		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? IdentityResult.Success : IIdentityRepository.NoChangesSaved;
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> RemoveClaimsAsync(BatUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
	{
		return await RemoveClaimsAsync(user, claims.Select(c => new IdentityUserClaim<string>
		{
			UserId = user.Id,
			ClaimType = c.Type,
			ClaimValue = c.Value
		}), cancellationToken);
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> CreateAsync(BatRole role, CancellationToken cancellationToken = default)
	{
		var entry = Roles.Add(role);
		entry.Entity.Touch();
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? IdentityResult.Success : IIdentityRepository.NoChangesSaved;
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> CreateIfNotExistsAsync(BatRole role, CancellationToken cancellationToken = default)
	{
		if (await GetRoleByIDAsync(role.Id, cancellationToken: cancellationToken) != null) return IdentityResult.Success;
		if (await GetRoleByNameAsync(role.Name??string.Empty, cancellationToken: cancellationToken) != null) return IdentityResult.Success;
		return await CreateAsync(role, cancellationToken);
	}

	private async ValueTask<BatRole?> PostFetchRole(BatRole? role, RoleFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		if (role is null || options is null || cancellationToken.IsCancellationRequested) return role;
		role.Claims = options.IncludeClaims ? await GetClaimsAsync(role, cancellationToken) : null;
		return role;
	}

	/// <inheritdoc/>
	public async ValueTask<BatRole?> GetRoleByIDAsync(string roleId, RoleFetchOptions? options = null, CancellationToken cancellationToken = default)
	{
		var role = await Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
		return await PostFetchRole(role, options, cancellationToken);
	}

	/// <inheritdoc/>
	public async ValueTask<BatRole?> GetRoleByNameAsync(string roleName, RoleFetchOptions? options = null, CancellationToken cancellationToken = default)
	{
		var role = await Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
		return await PostFetchRole(role, options, cancellationToken);
	}

	/// <inheritdoc/>
	public IAsyncEnumerable<BatRole> AllRolesAsync() => Roles.AsAsyncEnumerable();

	/// <inheritdoc/>
	public async ValueTask<BatRole?> UpdateAsync(BatRole role, CancellationToken cancellationToken = default)
	{
		var entry = Roles.Update(role);
		entry.Entity.Touch();
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? entry.Entity : null;
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> DeleteAsync(BatRole role, CancellationToken cancellationToken = default)
	{
		Roles.Remove(role);
		await SaveChangesAsync(cancellationToken);
		return IdentityResult.Success;
	}

	/// <inheritdoc/>
	public async ValueTask<IEnumerable<IdentityRoleClaim<string>>> GetClaimsAsync(BatRole role, CancellationToken cancellationToken = default)
	{
		return await RoleClaims
			.Where(rc => rc.RoleId == role.Id)
			.ToListAsync(cancellationToken) ?? [];
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> AddClaimsAsync(BatRole role, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
	{
		RoleClaims.AddRange(claims.Select(c => new IdentityRoleClaim<string>
		{
			RoleId = role.Id,
			ClaimType = c.Type,
			ClaimValue = c.Value
		}));
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? IdentityResult.Success : IIdentityRepository.NoChangesSaved;
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> AddClaimIfNotExistsAsync(BatRole role, Claim claim, CancellationToken cancellationToken = default)
	{
		var existing = await RoleClaims.FirstOrDefaultAsync(
			rc => rc.RoleId == role.Id && rc.ClaimType == claim.Type && rc.ClaimValue == claim.Value,
			cancellationToken);
		return existing is not null ? IdentityResult.Success : await AddClaimsAsync(role, [claim], cancellationToken);
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> RemoveClaimsAsync(BatRole role, IEnumerable<IdentityRoleClaim<string>> claims, CancellationToken cancellationToken = default)
	{
		var claimsList = claims.ToList(); // Convert to list to avoid multiple enumerations
		var roleClaims = role.Claims ?? await GetClaimsAsync(role, cancellationToken);
		foreach (var rc in roleClaims)
		{
			if (claimsList.Any(c => c.RoleId == role.Id && c.ClaimType == rc.ClaimType && c.ClaimValue == rc.ClaimValue))
			{
				RoleClaims.Remove(rc);
			}
		}

		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? IdentityResult.Success : IIdentityRepository.NoChangesSaved;
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> RemoveClaimsAsync(BatRole role, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
	{
		return await RemoveClaimsAsync(role, claims.Select(c => new IdentityRoleClaim<string>
		{
			RoleId = role.Id,
			ClaimType = c.Type,
			ClaimValue = c.Value
		}), cancellationToken);
	}
}
