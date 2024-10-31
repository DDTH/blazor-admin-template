using Bat.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Bat.Shared.EF;

public abstract class GenericDbContextRepository<T, TEntity, TKey> : DbContext, IGenericRepository<TEntity, TKey>
	where T : DbContext
	where TEntity : Entity<TKey>, new()
	where TKey : IEquatable<TKey>
{
	protected DbSet<TEntity> DbSet { get; set; } = default!;
	protected IEntityTypeConfiguration<TEntity>? EntityTypeConfiguration { get; set; }

	public GenericDbContextRepository(DbContextOptions<T> options) : this(options, default) { }

	public GenericDbContextRepository(
		DbContextOptions<T> options,
		IEntityTypeConfiguration<TEntity>? entityTypeConfiguration) : base(options)
	{
		EntityTypeConfiguration = entityTypeConfiguration;
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		if (EntityTypeConfiguration != null)
		{
			modelBuilder.ApplyConfiguration(EntityTypeConfiguration);
		}
	}

	protected virtual TEntity PrepareForCreate(TEntity t)
	{
		var clone = (TEntity)t.Clone();
		clone.CreatedAt = clone.UpdatedAt = DateTime.UtcNow;
		clone.ConcurrencyStamp = Guid.NewGuid().ToString();
		return clone;
	}

	/// <inheritdoc/>
	public virtual TEntity Create(TEntity t)
	{
		var result = DbSet.Add(PrepareForCreate(t));
		SaveChanges();
		return result.Entity;
	}

	/// <inheritdoc/>
	public virtual async ValueTask<TEntity> CreateAsync(TEntity t, CancellationToken cancellationToken = default)
	{
		var result = DbSet.Add(PrepareForCreate(t));
		await SaveChangesAsync(cancellationToken);
		return result.Entity;
	}

	/// <inheritdoc/>
	public virtual TEntity? GetByID(TKey id)
		=> DbSet.Find(id);

	/// <inheritdoc/>
	public virtual async ValueTask<TEntity?> GetByIDAsync(TKey id, CancellationToken cancellationToken = default)
		=> await DbSet.FindAsync([id], cancellationToken: cancellationToken);

	/// <inheritdoc/>
	public virtual IEnumerable<TEntity> GetAll()
		=> DbSet;

	/// <inheritdoc/>
	public virtual IAsyncEnumerable<TEntity> GetAllAsync()
		=> DbSet.AsAsyncEnumerable();

	protected virtual TEntity PrepareForUpdate(TEntity t)
	{
		t.UpdatedAt = DateTime.UtcNow;
		t.ConcurrencyStamp = Guid.NewGuid().ToString();
		return t;
	}

	/// <inheritdoc/>
	public virtual TEntity? Update(TEntity t)
	{
		var result = DbSet.Update(PrepareForUpdate(t));
		return SaveChanges() > 0 ? result.Entity : null;
	}

	/// <inheritdoc/>
	public virtual async ValueTask<TEntity?> UpdateAsync(TEntity t, CancellationToken cancellationToken = default)
	{
		var result = DbSet.Update(PrepareForUpdate(t));
		return await SaveChangesAsync(cancellationToken) > 0 ? result.Entity : null;
	}

	/// <inheritdoc/>
	public virtual bool Delete(TEntity t)
	{
		DbSet.Remove(t);
		return SaveChanges() > 0;
	}

	/// <inheritdoc/>
	public virtual async ValueTask<bool> DeleteAsync(TEntity t, CancellationToken cancellationToken = default)
	{
		DbSet.Remove(t);
		return await SaveChangesAsync(cancellationToken) > 0;
	}
}
