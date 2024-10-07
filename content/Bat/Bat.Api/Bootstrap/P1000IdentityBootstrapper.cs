using Bat.Api.Helpers;
using Bat.Shared.Bootstrap;
using Bat.Shared.EF.Identity;
using Bat.Shared.Identity;
using Ddth.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bat.Api.Bootstrap;

sealed class Defaults
{
	public static PasswordOptions passwordOptions = new()
	{
		RequiredLength = 12,
		RequiredUniqueChars = 5,
		RequireDigit = true,
		RequireLowercase = true,
		RequireUppercase = true,
		RequireNonAlphanumeric = false,
	};

	public static ClaimsIdentityOptions claimsIdentityOptions = new()
	{
		EmailClaimType = "ema",
		RoleClaimType = "rol",
		UserIdClaimType = "uid",
		UserNameClaimType = "una",
		SecurityStampClaimType = "sec",
	};
}

/// <summary>
/// Built-in bootstrapper that initializes Asp.Net Core Identity services.
/// </summary>
[Bootstrapper]
public class IdentityBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<IdentityBootstrapper>();
		logger.LogInformation("Configuring Identity services...");

		const string confKeyBase = "Databases:Identity";
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
			appBuilder.Services.AddDbContext<IIdentityRepository, IdentityDbContextRepository>(optionsAction);
		else
			appBuilder.Services.AddDbContextPool<IIdentityRepository, IdentityDbContextRepository>(
				optionsAction, dbConf.PoolSize > 0 ? dbConf.PoolSize : DbConf.DEFAULT_POOL_SIZE);

		// https://github.com/dotnet/aspnetcore/issues/26119
		// Use .AddIdentityCore<User> then add necessary services manually (e.g. AddRoles, AddSignInManager, etc.)
		// instead of using .AddIdentity<User, Role>
		appBuilder.Services
			.AddIdentityCore<BatUser>(opts =>
			{
				opts.Password = Defaults.passwordOptions;
				opts.ClaimsIdentity = Defaults.claimsIdentityOptions;
			})
			//.AddRoles<BatRole>()
			//.AddSignInManager<SignInManager<BatUser>>()
			.AddEntityFrameworkStores<IdentityDbContextRepository>()
			;

		// perform initialization tasks in background
		appBuilder.Services.AddHostedService<IdentityInitializer>();
	}
}

sealed class IdentityInitializer(
	IServiceProvider serviceProvider,
	ILogger<IdentityInitializer> logger,
	IWebHostEnvironment environment) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Initializing identity data...");

		using (var scope = serviceProvider.CreateScope())
		{
			var identityRepo = scope.ServiceProvider.GetRequiredService<IIdentityRepository>() as IdentityDbContextRepository
				?? throw new InvalidOperationException("Identity repository is not an instance of IdentityDbContextRepository.");
			var tryParseInitDb = bool.TryParse(Environment.GetEnvironmentVariable(Globals.ENV_INIT_DB), out var initDb);
			if (environment.IsDevelopment() || (tryParseInitDb && initDb))
			{
				logger.LogInformation("Ensuring database schema exist...");
				identityRepo.Database.EnsureCreated();
			}

			var nameNormalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>()
				?? throw new InvalidOperationException("LookupNormalizer service is not registered.");

			logger.LogInformation("Ensuring built-in roles exist and permissions setup...");
			foreach (var r in BatRole.ALL_BUILTIN_ROLES)
			{
				r.NormalizedName = nameNormalizer.NormalizeName(r.Name);
				var result = await identityRepo.CreateIfNotExistsAsync(r, cancellationToken: cancellationToken);
				if (result != IdentityResult.Success)
				{
					throw new InvalidOperationException(result.ToString());
				}

				// ensure claims for the role
				if (r.Claims != null)
				{
					foreach (var c in r.Claims)
					{
						var resultClaim = await identityRepo.AddClaimIfNotExistsAsync(r, c.ToClaim(), cancellationToken: cancellationToken);
						if (resultClaim != IdentityResult.Success)
						{
							throw new InvalidOperationException(resultClaim.ToString());
						}
					}
				}
			}

			logger.LogInformation("Seeding user accounts...");
			var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<BatUser>>();
			var identityOptions = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>()?.Value;
			foreach (var u in BatUser.ALL_BUILTIN_USERS)
			{
				var user = await identityRepo.GetUserByIDAsync(u.Id, cancellationToken: cancellationToken);
				if (user == null)
				{
					var generatedPassword = RandomPasswordGenerator.GenerateRandomPassword(identityOptions?.Password);
					logger.LogWarning("User '{user}' does not exist. Creating one with a random password: {password}", u.UserName, generatedPassword);
					logger.LogWarning("PLEASE REMEMBER THIS PASSWORD AS IT WILL NOT BE DISPLAYED AGAIN!");

					if (string.IsNullOrEmpty(u.Id))
					{
						u.Id = Guid.NewGuid().ToString();
					}
					u.PasswordHash = passwordHasher.HashPassword(u, generatedPassword);
					u.NormalizedUserName = nameNormalizer.NormalizeName(u.UserName);
					u.NormalizedEmail = nameNormalizer.NormalizeEmail(u.Email);
					var result = await identityRepo.CreateIfNotExistsAsync(u, cancellationToken: cancellationToken);
					if (result != IdentityResult.Success)
					{
						throw new InvalidOperationException(result.ToString());
					}
				}

				// add roles to the user
				if (u.Roles != null)
				{
					foreach (var r in u.Roles)
					{
						var resultRole = await identityRepo.AddToRoleIfNotExistsAsync(u, r, cancellationToken: cancellationToken);
						if (resultRole != IdentityResult.Success)
						{
							throw new InvalidOperationException(resultRole.ToString());
						}
					}
				}

				// add claims to the user
				if (u.Claims != null)
				{
					foreach (var c in u.Claims)
					{
						var resultClaim = await identityRepo.AddClaimIfNotExistsAsync(u, c.ToClaim(), cancellationToken: cancellationToken);
						if (resultClaim != IdentityResult.Success)
						{
							throw new InvalidOperationException(resultClaim.ToString());
						}
					}
				}
			}
		}
	}
}
