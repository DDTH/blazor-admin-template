using Microsoft.Extensions.Caching.Distributed;
using System.IO.Compression;

namespace Bat.Shared.Cache;

/// <summary>
/// Options for the <see cref="CacheFacade"/>.
/// </summary>
public sealed class CacheFacadeOptions
{
	public static CacheFacadeOptions DEFAULT { get; set; } = new CacheFacadeOptions();

	public DistributedCacheEntryOptions DefaultDistributedCacheEntryOptions { get; set; } = new DistributedCacheEntryOptions();
	public ICacheEntrySerializer CacheEntrySerializer { get; set; } = new JsonCacheEntrySerializer();
	public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.NoCompression;
	public string KeyPrefix { get; set; } = "";
}

/// <summary>
/// Reference implementation of <see cref="ICacheFacade{TCategory}"/>.
/// </summary>
/// <typeparam name="TCategory"></typeparam>
public sealed class CacheFacade<TCategory> : ICacheFacade<TCategory>
{
	private readonly IDistributedCache dcache;
	private readonly ICacheEntrySerializer serializer;
	private readonly DistributedCacheEntryOptions defaultOptions;
	private readonly ICompressor compressor;
	private readonly string keyPrefix;

	public CacheFacade(IDistributedCache distributedCache) : this(distributedCache, CacheFacadeOptions.DEFAULT) { }

	public CacheFacade(IDistributedCache distributedCache, CacheFacadeOptions options)
	{
		ArgumentNullException.ThrowIfNull(distributedCache, nameof(distributedCache));

		this.dcache = distributedCache;
		this.serializer = options?.CacheEntrySerializer ?? JsonCacheEntrySerializer.DEFAULT;
		this.defaultOptions = options?.DefaultDistributedCacheEntryOptions ?? new DistributedCacheEntryOptions();
		this.compressor = options == null || options.CompressionLevel == CompressionLevel.NoCompression
			? new NoCompressionCompressor()
			: options.CompressionLevel == CompressionLevel.Fastest || options.CompressionLevel == CompressionLevel.Optimal
				? new BrotliCompressor(options.CompressionLevel)
				: new DeflateCompressor(options.CompressionLevel);
		this.keyPrefix = options?.KeyPrefix ?? "";
	}

	/// <inheritdoc/>
	public byte[]? Get(string key)
	{
		var cached = dcache.Get($"{keyPrefix}{key}");
		return cached == null ? null : compressor.DecompressAsync(cached).Result;
	}

	/// <inheritdoc/>
	public async Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
	{
		var cached = await dcache.GetAsync($"{keyPrefix}{key}", cancellationToken);
		return cached == null ? null : await compressor.DecompressAsync(cached, cancellationToken);
	}

	/// <inheritdoc/>
	public T? Get<T>(string key)
	{
		var cached = Get(key);
		return cached == null ? default : serializer.Deserialize<T>(cached);
	}

	/// <inheritdoc/>
	public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
	{
		var cached = await GetAsync(key, cancellationToken);
		return cached == null ? default : await serializer.DeserializeAsync<T>(cached, cancellationToken);
	}

	/// <inheritdoc/>
	public void Refresh(string key)
	{
		dcache.Refresh($"{keyPrefix}{key}");
	}

	/// <inheritdoc/>
	public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
	{
		await dcache.RefreshAsync($"{keyPrefix}{key}", cancellationToken);
	}

	/// <inheritdoc/>
	public void Remove(string key)
	{
		dcache.Remove($"{keyPrefix}{key}");
	}

	/// <inheritdoc/>
	public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
	{
		await dcache.RemoveAsync($"{keyPrefix}{key}", cancellationToken);
	}

	/// <inheritdoc/>
	public void Set(string key, byte[] value, DistributedCacheEntryOptions options = default!)
	{
		dcache.Set($"{keyPrefix}{key}", compressor.CompressAsync(value).Result, options ?? defaultOptions);
	}

	/// <inheritdoc/>
	public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options = default!, CancellationToken cancellationToken = default)
	{
		await dcache.SetAsync($"{keyPrefix}{key}", await compressor.CompressAsync(value, cancellationToken), options ?? defaultOptions, cancellationToken);
	}

	/// <inheritdoc/>
	public void Set<T>(string key, T value, DistributedCacheEntryOptions options = default!)
	{
		Set(key, serializer.Serialize(value), options);
	}

	/// <inheritdoc/>
	public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options = default!, CancellationToken cancellationToken = default)
	{
		await SetAsync(key, await serializer.SerializeAsync(value, cancellationToken), options, cancellationToken);
	}
}
