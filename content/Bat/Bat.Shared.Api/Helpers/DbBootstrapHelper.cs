using Microsoft.EntityFrameworkCore;

namespace Bat.Shared.Api.Helpers;

/// <summary>
/// Represents the database configuration bock in the appsettings.json
/// </summary>
public class DbConf
{
	public const bool DEFAULT_USE_DB_CONTEXT_POOL = false;
	public const int DEFAULT_POOL_SIZE = 128;

	/// <summary>
	/// The type of database backend.
	/// </summary>
	public DbType Type { get; set; }

	/// <summary>
	/// Point to the connection string defined in the ConnectionStrings block.
	/// </summary>
	public string ConnectionString { get; set; } = String.Empty;

	/// <summary>
	/// Should the DbContext be pooled?
	/// </summary>
	public bool UseDbContextPool { get; set; } = DEFAULT_USE_DB_CONTEXT_POOL;

	/// <summary>
	/// Maximum number of DbContext instances in the pool.
	/// </summary>
	public int PoolSize { get; set; } = DEFAULT_POOL_SIZE;
}

public enum DbType
{
	NULL,
	INMEMORY, MEMORY,
	SQLITE,
	SQLSERVER,
}

public static class DbBootstrapHelper
{
	public static void ConfigureDbContext<TContextService, TContextImplementation>(
		WebApplicationBuilder appBuilder,
		string confKeyBase,
		ILogger logger) where TContextService : class where TContextImplementation : DbContext, TContextService
	{
		var services = appBuilder.Services;
		var confManager = appBuilder.Configuration;
		var env = appBuilder.Environment;
		var dbConf = confManager.GetSection(confKeyBase).Get<DbConf>()
			?? throw new InvalidDataException($"No configuration found at key {confKeyBase} in the configurations.");
		void optionsAction(DbContextOptionsBuilder options)
		{
			if (env.IsDevelopment())
			{
				options.EnableDetailedErrors().EnableSensitiveDataLogging();
			}
			if (dbConf.Type == DbType.NULL)
			{
				logger.LogWarning("Unknown value at key {conf} in the configurations. Defaulting to INMEMORY.", $"{confKeyBase}:Type");
				dbConf.Type = DbType.INMEMORY;
			}

			var connStr = confManager.GetConnectionString(dbConf.ConnectionString) ?? "";
			switch (dbConf.Type)
			{
				case DbType.INMEMORY or DbType.MEMORY:
					options.UseInMemoryDatabase(confKeyBase);
					break;
				case DbType.SQLITE or DbType.SQLSERVER:
					if (string.IsNullOrWhiteSpace(dbConf.ConnectionString))
					{
						throw new InvalidDataException($"No connection string name found at key {confKeyBase}:ConnectionString in the configurations.");
					}
					if (string.IsNullOrWhiteSpace(connStr))
					{
						throw new InvalidDataException($"No connection string {dbConf.ConnectionString} defined in the ConnectionStrings section in the configurations.");
					}
					if (dbConf.Type == DbType.SQLITE)
						options.UseSqlite(connStr);
					else if (dbConf.Type == DbType.SQLSERVER)
						options.UseSqlServer(connStr);
					break;
				default:
					throw new InvalidDataException($"Invalid value at key {confKeyBase}:Type in the configurations: '{dbConf.Type}'");
			}
		}
		if (dbConf.UseDbContextPool)
			services.AddDbContext<TContextService, TContextImplementation>(optionsAction);
		else
			services.AddDbContextPool<TContextService, TContextImplementation>(
				optionsAction, dbConf.PoolSize > 0 ? dbConf.PoolSize : DbConf.DEFAULT_POOL_SIZE);
	}
}