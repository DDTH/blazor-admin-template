using Bat.Api.Helpers;
using Bat.Shared.Bootstrap;
using Bat.Shared.EF;
using Bat.Shared.EF.Identity;
using Bat.Shared.Identity;
using Bat.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Bat.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that initializes DbContext/DbContextPool services.
/// </summary>
[Bootstrapper]
public class DbContextBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DbContextBootstrapper>();
		logger.LogInformation("Configuring DbContext services...");

		ConfigureDbContext<IApplicationRepository, ApplicationDbContextRepository>(appBuilder, "Databases:Application", logger);
		ConfigureDbContext<IIdentityRepository, IdentityDbContextRepository>(appBuilder, "Databases:Identity", logger);

		appBuilder.Services.AddHostedService<ApplicationInitializer>();
		appBuilder.Services.AddHostedService<IdentityInitializer>();
	}

	private static void ConfigureDbContext<TContextService, TContextImplementation>(
		WebApplicationBuilder appBuilder,
		string confKeyBase,
		ILogger logger) where TContextService : class where TContextImplementation : DbContext, TContextService
	{
		var dbConf = appBuilder.Configuration.GetSection(confKeyBase).Get<DbConf>()
			?? throw new InvalidDataException($"No configuration found at key {confKeyBase} in the configurations.");
		void optionsAction(DbContextOptionsBuilder options)
		{
			if (appBuilder.Environment.IsDevelopment())
			{
				options.EnableDetailedErrors().EnableSensitiveDataLogging();
			}
			if (dbConf.Type == DbType.NULL)
			{
				logger.LogWarning("Unknown value at key {conf} in the configurations. Defaulting to INMEMORY.", $"{confKeyBase}:Type");
				dbConf.Type = DbType.INMEMORY;
			}

			var connStr = appBuilder.Configuration.GetConnectionString(dbConf.ConnectionString) ?? "";
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
			appBuilder.Services.AddDbContext<TContextService, TContextImplementation>(optionsAction);
		else
			appBuilder.Services.AddDbContextPool<TContextService, TContextImplementation>(
				optionsAction, dbConf.PoolSize > 0 ? dbConf.PoolSize : DbConf.DEFAULT_POOL_SIZE);
	}
}
