using Bat.Shared.Cache;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

	/// <summary>
	/// General error when no changes are saved.
	/// </summary>
	public static readonly IdentityResult NoChangesSaved = IdentityResult.Failed(new IdentityError() { Code = "NoChangesSaved" });

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> CreateAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		var entry = Users.Add(user);
		entry.Entity.SecurityStamp = Guid.NewGuid().ToString("N");
		entry.Entity.Touch();
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0
			? IdentityResult.Success
			: NoChangesSaved;
	}

	/// <inheritdoc/>
	public async ValueTask<IdentityResult> CreateIfNotExistsAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		if (await GetUserByIDAsync(user.Id, cancellationToken: cancellationToken) != null) return IdentityResult.Success;
		return await CreateAsync(user, cancellationToken);
	}

	private async ValueTask<BatUser?> PostFetchUser(BatUser? user, UserFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		if (user is null || options is null || cancellationToken.IsCancellationRequested) return user;
		user.Roles = options.IncludeRoles ? await GetRolesAsync(user, cancellationToken) : null;
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
	public async ValueTask<IEnumerable<BatRole>> GetRolesAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		return await UserRoles
			.Where(ur => ur.UserId == user.Id)
			.Join(Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
			.ToListAsync(cancellationToken) ?? [];
	}

	/// <inheritdoc/>
	public async ValueTask<IEnumerable<IdentityUserClaim<string>>> GetClaimsAsync(BatUser user, CancellationToken cancellationToken = default)
	{
		return await UserClaims
			.Where(uc => uc.UserId == user.Id)
			.ToListAsync(cancellationToken) ?? [];
	}

	public ValueTask<bool> HasRoleAsync(BatUser user, BatRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<bool> HasRoleAsync(BatUser user, string roleName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<IdentityResult> AddToRolesAsync(BatUser user, IEnumerable<BatRole> roles, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<IdentityResult> AddToRolesAsync(BatUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<IdentityResult> RemoveFromRolesAsync(BatUser user, IEnumerable<BatRole> roles, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<IdentityResult> RemoveFromRolesAsync(BatUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<IdentityResult> AddClaimsAsync(BatUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();

	/*----------------------------------------------------------------------*/

	public ValueTask<IdentityResult> CreateAsync(BatRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<IdentityResult> CreateIfNotExistsAsync(BatRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<BatRole?> GetRoleByIDAsync(string roleId, RoleFetchOptions? options = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<BatRole?> GetRoleByNameAsync(string roleName, RoleFetchOptions? options = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<IEnumerable<IdentityRoleClaim<string>>> GetClaimsAsync(BatRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public ValueTask<IdentityResult> AddClaimsAsync(BatRole role, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
