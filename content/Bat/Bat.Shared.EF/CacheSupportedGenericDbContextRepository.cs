using Bat.Shared.Cache;
using Bat.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bat.Shared.EF;

public abstract class CacheSupportedGenericDbContextRepository<T, TEntity, TKey> : GenericDbContextRepository<T, TEntity, TKey>
	where T : DbContext
	where TEntity : Entity<TKey>, new()
	where TKey : IEquatable<TKey>
{
	protected ICacheFacade<TEntity>? Cache { get; set; }

	public CacheSupportedGenericDbContextRepository(DbContextOptions<T> options)
		: this(options, default) { }

	public CacheSupportedGenericDbContextRepository(DbContextOptions<T> options, IEntityTypeConfiguration<TEntity>? entityTypeConfiguration)
		: this(options, entityTypeConfiguration, default) { }

	public CacheSupportedGenericDbContextRepository(
		DbContextOptions<T> options,
		IEntityTypeConfiguration<TEntity>? entityTypeConfiguration,
		ICacheFacade<TEntity>? cache) : base(options, entityTypeConfiguration)
	{
		Cache = cache;
		TrackStateChange();
	}

	protected CacheSupportedGenericDbContextRepository<T, TEntity, TKey> AddStateChangeHandler(EventHandler<EntityStateChangedEventArgs> eventHandler)
	{
		ChangeTracker.StateChanged += eventHandler;
		return this;
	}

	protected CacheSupportedGenericDbContextRepository<T, TEntity, TKey> RemoveStateChangeHandler(EventHandler<EntityStateChangedEventArgs> eventHandler)
	{
		ChangeTracker.StateChanged -= eventHandler;
		return this;
	}

	private EventHandler<EntityStateChangedEventArgs> StateChangedChangeTracker = default!;

	/// <summary>
	/// Tracks changes in the entity state and updates the cache accordingly.
	/// </summary>
	private void TrackStateChange()
	{
		StateChangedChangeTracker = async (sender, args) =>
		{
			if (args.Entry.Entity is TEntity entity)
			{
				switch (args.Entry.State)
				{
					case EntityState.Added or EntityState.Modified or EntityState.Unchanged:
						if (args.Entry.State != EntityState.Unchanged)
						{
							entity.Touch();
						}
						if (Cache != null)
							await Cache.SetAsync(entity.Id.ToString()!, entity, default!);
						break;
					default:
						if (Cache != null)
							await Cache.RemoveAsync(entity.Id.ToString()!);
						break;
				}
			}
		};
		ChangeTracker.StateChanged += StateChangedChangeTracker;
	}

	/// <summary>
	/// Called when a cache hit occurs.
	/// </summary>
	/// <param name="cached"></param>
	/// <returns></returns>
	protected virtual TEntity CacheHit(TEntity cached)
	{
		return Attach(cached).Entity;
	}

	/// <summary>
	/// Called when a cache miss occurs, via non-async methods.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	protected virtual TEntity? CacheMiss(TEntity? item)
	{
		if (item != null && Cache != null)
		{
			Cache.Set(item.Id.ToString()!, item, default!);
		}
		return item;
	}

	/// <summary>
	/// Called when a cache miss occurs, via async methods.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	protected virtual async ValueTask<TEntity?> CacheMissAsync(TEntity? item, CancellationToken cancellationToken = default)
	{
		if (item != null && Cache != null)
		{
			await Cache.SetAsync(item.Id.ToString()!, item, cancellationToken: cancellationToken);
		}
		return item;
	}

	/// <inheritdoc/>
	public override TEntity? GetByID(TKey id)
	{
		var cached = Cache?.Get<TEntity>(id.ToString()!);
		return cached != null
			? CacheHit(cached)
			: CacheMiss(base.GetByID(id));
	}

	/// <inheritdoc/>
	public override async ValueTask<TEntity?> GetByIDAsync(TKey id, CancellationToken cancellationToken = default)
	{
		var cached = Cache != null ? await Cache.GetAsync<TEntity>(id.ToString()!, cancellationToken: cancellationToken) : null;
		return cached != null
			? CacheHit(cached)
			: await CacheMissAsync(await base.GetByIDAsync(id, cancellationToken: cancellationToken), cancellationToken: cancellationToken);
	}
}
