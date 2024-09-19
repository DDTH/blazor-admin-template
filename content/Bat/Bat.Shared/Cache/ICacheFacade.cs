using Microsoft.Extensions.Caching.Distributed;

namespace Bat.Shared.Cache;

/// <summary>
/// Extends <see cref="IDistributedCache"/> functionality with typed methods.
/// </summary>
/// <typeparam name="TCategory"></typeparam>
public interface ICacheFacade<TCategory> : IDistributedCache
{
	/// <summary>
	/// Gets a value with the given key.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <returns></returns>
	T? Get<T>(string key);

	/// <summary>
	/// Gets a value with the given key asynchronously.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets a value with the given key. 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="options"></param>
	void Set<T>(string key, T value, DistributedCacheEntryOptions options = default!);

	/// <summary>
	/// Sets a value with the given key asynchronously.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="options"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options = default!, CancellationToken cancellationToken = default);
}
