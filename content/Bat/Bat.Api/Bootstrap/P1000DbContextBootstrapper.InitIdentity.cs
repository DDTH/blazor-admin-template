using System.Text.Json;
using Bat.Shared.EF.Identity;
using Bat.Shared.Identity;
using Ddth.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Bat.Api.Bootstrap;

sealed class IdentityInitializer(
	IConfiguration appConfig,
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

			await SeedRoles(dbContext, nameNormalizer, cancellationToken);

			var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<BatUser>>();
			var identityOptions = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>()?.Value!;
			await SeedUsers(dbContext, nameNormalizer, identityOptions, passwordHasher, cancellationToken);
		}
	}

	struct SeedingRole
	{
		public string? Id { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public IEnumerable<string>? Claims { get; set; }
	}

	private async Task SeedRoles(IIdentityRepository dbContext, ILookupNormalizer lookupNormalizer, CancellationToken cancellationToken)
	{
		logger.LogInformation("Seeding roles...");
		var seedRoles = appConfig.GetSection("SeedingData:Identity:Roles").Get<IEnumerable<SeedingRole>>() ?? [];
		foreach (var r in seedRoles)
		{
			if (string.IsNullOrEmpty(r.Name))
			{
				logger.LogWarning("Skipping invalid seeding role data - role name is required: {role}", JsonSerializer.Serialize(r));
				continue;
			}

			var role = new BatRole
			{
				Id = string.IsNullOrEmpty(r.Id) ? Guid.NewGuid().ToString() : r.Id.ToLower().Trim(),
				Name = r.Name,
				Description = r.Description,
			};
			role.NormalizedName = lookupNormalizer.NormalizeName(role.Name);

			// create the role
			var result = await dbContext.CreateIfNotExistsAsync(role, cancellationToken: cancellationToken);
			if (result != IdentityResult.Success)
			{
				throw new InvalidOperationException(result.ToString());
			}
			role = await dbContext.GetRoleByNameAsync(role.Name, cancellationToken: cancellationToken)
				?? throw new InvalidOperationException($"Role '{role.Name}' is not found after creation.");

			// add claims to the role
			var seedClaims = r.Claims?.Select(IdentityClaim.CreateFrom).Where(c => c != null && BuiltinClaims.ClaimExists((IdentityClaim)c!)) ?? [];
			foreach (var c in seedClaims)
			{
				var iclaim = (IdentityClaim)c!;
				var resultClaim = await dbContext.AddClaimIfNotExistsAsync(role, new Claim(iclaim.Type, iclaim.Value), cancellationToken: cancellationToken);
				if (resultClaim != IdentityResult.Success)
				{
					throw new InvalidOperationException(resultClaim.ToString());
				}
			}
		}
	}

	struct SeedingUser
	{
		public string? Id { get; set; }
		public string? UserName { get; set; }
		public string? Email { get; set; }
		public string? GivenName { get; set; }
		public string? FamilyName { get; set; }
		public IEnumerable<string>? Roles { get; set; }
		public IEnumerable<string>? Claims { get; set; }
	}

	private async Task SeedUsers(IIdentityRepository dbContext, ILookupNormalizer lookupNormalizer, IdentityOptions identityOptions, IPasswordHasher<BatUser> passwordHasher, CancellationToken cancellationToken)
	{
		logger.LogInformation("Seeding user accounts...");
		var seedUsers = appConfig.GetSection("SeedingData:Identity:Users").Get<IEnumerable<SeedingUser>>() ?? [];
		foreach (var u in seedUsers)
		{
			if (string.IsNullOrEmpty(u.UserName) || string.IsNullOrEmpty(u.Email))
			{
				logger.LogWarning("Skipping invalid seeding user data - user name and email are required: {user}", JsonSerializer.Serialize(u));
				continue;
			}
			var id = string.IsNullOrEmpty(u.Id) ? Guid.NewGuid().ToString() : u.Id.ToLower().Trim();
			var username = u.UserName.ToLower().Trim();
			var email = u.Email.ToLower().Trim();
			var user = await dbContext.GetUserByEmailAsync(email, cancellationToken: cancellationToken)
				?? await dbContext.GetUserByUserNameAsync(username, cancellationToken: cancellationToken)
				?? await dbContext.GetUserByIDAsync(id, cancellationToken: cancellationToken);
			if (user == null)
			{
				var generatedPassword = RandomPasswordGenerator.GenerateRandomPassword(identityOptions?.Password);
				logger.LogWarning("User '{user}' does not exist. Creating one with email '{email}' and a random password: {password}", u.UserName, u.Email, generatedPassword);
				logger.LogWarning("PLEASE REMEMBER THIS PASSWORD AS IT WILL NOT BE DISPLAYED AGAIN!");

				// FIXME: NOT TO USE THIS IN PRODUCTION!
				// for demo purpose: store the generated password in environment variables
				// if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
				if (environment.IsDevelopment())
				{
					logger.LogCritical("Storing the generated password in environment variables for demo purpose. DO NOT USE THIS IN PRODUCTION!");
					Environment.SetEnvironmentVariable($"USER_SECRET_I_{id}", generatedPassword);
					logger.LogCritical("User secret for '{id}': {secret}", $"USER_SECRET_I_{id}", generatedPassword);
				}

				user = new BatUser
				{
					Id = id,
					UserName = username,
					NormalizedUserName = lookupNormalizer.NormalizeName(username),
					Email = email,
					NormalizedEmail = lookupNormalizer.NormalizeEmail(email),
					GivenName = u.GivenName?.Trim(),
					FamilyName = u.FamilyName?.Trim(),
				};
				user.PasswordHash = passwordHasher.HashPassword(user, generatedPassword);
				var result = await dbContext.CreateIfNotExistsAsync(user, cancellationToken: cancellationToken);
				if (result != IdentityResult.Success)
				{
					throw new InvalidOperationException(result.ToString());
				}
			}

			// add roles to the user
			var userRoles = u.Roles?.Where(r => !string.IsNullOrEmpty(r)).Select(r => dbContext.GetRoleByNameAsync(r).Result).Where(r => r != null) ?? [];
			foreach (var r in userRoles)
			{
				var resultRole = await dbContext.AddToRoleIfNotExistsAsync(user, r!, cancellationToken: cancellationToken);
				if (resultRole != IdentityResult.Success)
				{
					throw new InvalidOperationException(resultRole.ToString());
				}
			}

			// add claims to the user
			var seedClaims = u.Claims?.Select(IdentityClaim.CreateFrom).Where(c => c != null && BuiltinClaims.ClaimExists((IdentityClaim)c!)) ?? [];
			foreach (var c in seedClaims)
			{
				var iclaim = (IdentityClaim)c!;
				var resultClaim = await dbContext.AddClaimIfNotExistsAsync(user, new Claim(iclaim.Type, iclaim.Value), cancellationToken: cancellationToken);
				if (resultClaim != IdentityResult.Success)
				{
					throw new InvalidOperationException(resultClaim.ToString());
				}
			}
		}
	}
}
