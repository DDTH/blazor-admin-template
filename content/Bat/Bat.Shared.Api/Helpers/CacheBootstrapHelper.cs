using System.IO.Compression;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace Bat.Shared.Api.Helpers;

/// <summary>
/// Represents the cache configuration block in the appsettings.json
/// </summary>
public class CacheConf
{
	public const int DEFAULT_SIZE_LIMIT = 100 * 1024 * 1024; // ~100mb

	/// <summary>
	/// The type of cache to use.
	/// </summary>
	public CacheType Type { get; set; }

	/// <summary>
	/// Cache key prefix (Redis caches only).
	/// </summary>
	public string KeyPrefix { get; set; } = String.Empty;

	/// <summary>
	/// The size limit of the cache (in bytes, in-memory caches only).
	/// </summary>
	public int SizeLimit { get; set; } = DEFAULT_SIZE_LIMIT;

	/// <summary>
	/// Point to the connection string defined in the ConnectionStrings block (Redis caches only).
	/// </summary>
	public string ConnectionString { get; set; } = String.Empty;

	/// <summary>
	/// Cache entries expire after the specified period, in seconds. Set to 0 to disable expiration.
	/// </summary>
	public int ExpirationAfterAccess { get; set; } = 0;

	/// <summary>
	/// Cache entries expire after the specified period of no access, in seconds. Set to 0 to disable expiration.
	/// </summary>
	public int ExpirationAfterWrite { get; set; } = 0;

	public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.NoCompression;
}

public enum CacheType
{
	NONE,
	INMEMORY, MEMORY,
	REDIS,
}

public static class CacheBootstrapHelper
{
	public static CacheConf? SetupCache(WebApplicationBuilder appBuilder, string confKeyBase, string keyedServiceName, ILogger logger)
	{
		var services = appBuilder.Services;
		var confManager = appBuilder.Configuration;
		var cacheConf = confManager.GetSection(confKeyBase).Get<CacheConf>()
			?? throw new InvalidDataException($"No configuration found at key {confKeyBase} in the configurations.");
		switch (cacheConf.Type)
		{
			case CacheType.INMEMORY or CacheType.MEMORY:
				var cacheSizeLimit = cacheConf.SizeLimit > 0 ? cacheConf.SizeLimit : CacheConf.DEFAULT_SIZE_LIMIT;
				logger.LogInformation("Using in-memory cache for {domain}, with SizeLimit = {SizeLimit}...", keyedServiceName, cacheSizeLimit);
				services.AddKeyedSingleton<IDistributedCache>(keyedServiceName, (sp, key) =>
				{
					var options = new MemoryDistributedCacheOptions()
					{
						SizeLimit = cacheSizeLimit
					};
					return new MemoryDistributedCache(Options.Create(options));
				});
				return cacheConf;
			case CacheType.REDIS:
				if (string.IsNullOrWhiteSpace(cacheConf.ConnectionString))
				{
					throw new InvalidDataException($"No connection string name found at key {confKeyBase}:ConnectionString in the configurations.");
				}
				var connStr = confManager.GetConnectionString(cacheConf.ConnectionString) ?? string.Empty;
				if (string.IsNullOrWhiteSpace(connStr))
				{
					throw new InvalidDataException($"No connection string {cacheConf.ConnectionString} defined in the ConnectionStrings section in the configurations.");
				}
				logger.LogInformation("Using Redis cache for {domain}...", keyedServiceName);
				services.AddKeyedSingleton<IDistributedCache>(keyedServiceName, (sp, key) =>
				{
					var options = new RedisCacheOptions()
					{
						Configuration = connStr
					};
					return new RedisCache(Options.Create(options));
				});
				return cacheConf;
			default:
				logger.LogInformation("No cache configured for {domain}, or invalid cache type '{cacheType}'.", keyedServiceName, cacheConf.Type);
				return null;
		}
	}
}