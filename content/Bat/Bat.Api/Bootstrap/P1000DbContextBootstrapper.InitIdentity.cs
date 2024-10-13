using Bat.Shared.EF.Identity;
using Bat.Shared.Identity;
using Ddth.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Bat.Api.Bootstrap;

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
			var dbContext = scope.ServiceProvider.GetRequiredService<IIdentityRepository>() as IdentityDbContextRepository
				?? throw new InvalidOperationException($"Identity repository is not an instance of {nameof(IdentityDbContextRepository)}.");
			var tryParseInitDb = bool.TryParse(Environment.GetEnvironmentVariable(Globals.ENV_INIT_DB), out var initDb);
			if (environment.IsDevelopment() || (tryParseInitDb && initDb))
			{
				logger.LogInformation("Ensuring database schema exist...");
				dbContext.Database.EnsureCreated();
			}

			var nameNormalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>()
				?? throw new InvalidOperationException("LookupNormalizer service is not registered.");

			logger.LogInformation("Ensuring built-in roles exist and permissions setup...");
			foreach (var r in BatRole.ALL_BUILTIN_ROLES)
			{
				r.NormalizedName = nameNormalizer.NormalizeName(r.Name);
				var result = await dbContext.CreateIfNotExistsAsync(r, cancellationToken: cancellationToken);
				if (result != IdentityResult.Success)
				{
					throw new InvalidOperationException(result.ToString());
				}

				// ensure claims for the role
				if (r.Claims != null)
				{
					foreach (var c in r.Claims)
					{
						var resultClaim = await dbContext.AddClaimIfNotExistsAsync(r, c.ToClaim(), cancellationToken: cancellationToken);
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
				var user = await dbContext.GetUserByIDAsync(u.Id, cancellationToken: cancellationToken);
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
					var result = await dbContext.CreateIfNotExistsAsync(u, cancellationToken: cancellationToken);
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
						var resultRole = await dbContext.AddToRoleIfNotExistsAsync(u, r, cancellationToken: cancellationToken);
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
						var resultClaim = await dbContext.AddClaimIfNotExistsAsync(u, c.ToClaim(), cancellationToken: cancellationToken);
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
