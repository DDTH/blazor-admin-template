using Bat.Api.Controllers;
using Bat.Shared.Api;
using Bat.Shared.Identity;
using Bat.Shared.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bat.Api.Services;

/// <summary>
/// JWT implementation of <see cref="IAuthenticator"/>.
/// </summary>
public sealed class SampleJwtAuthenticator(
	IServiceProvider serviceProvider,
	IOptions<JwtOptions> jwtOptions,
	IJwtService jwtService,
	IOptions<IdentityOptions> identityOptions,
	ILogger<SampleJwtAuthenticator> logger) : IAuthenticator, IAuthenticatorAsync
{
	private readonly int expirationSeconds = jwtOptions.Value.DefaultExpirationSeconds;
	private readonly string claimTypeEmail = identityOptions.Value.ClaimsIdentity.EmailClaimType;
	private readonly string claimTypeRole = identityOptions.Value.ClaimsIdentity.RoleClaimType;
	private readonly string claimTypeUserId = identityOptions.Value.ClaimsIdentity.UserIdClaimType;
	private readonly string claimTypeUserName = identityOptions.Value.ClaimsIdentity.UserNameClaimType;
	private readonly string claimTypeSecurity = identityOptions.Value.ClaimsIdentity.SecurityStampClaimType;
	private const string DefaultSecurityStampValue = "00000000";

	private async Task<string> GenerateJwtToken(IIdentityRepository identityRepo, BatUser user, DateTime expiry)
	{
		// add base claims
		var authClaims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new(claimTypeUserId, user.Id),
			new(claimTypeUserName, user.UserName ?? ""),
			new(claimTypeEmail, user.Email?? ""),
			new(claimTypeSecurity, user.SecurityStamp?.Substring(user.SecurityStamp.Length - 8) ?? DefaultSecurityStampValue)
		};

		// add roles
		var userRoles = user.Roles ?? await identityRepo.GetRolesAsync(user, RoleFetchOptions.DEFAULT.FetchClaims());
		authClaims.AddRange(userRoles.Select(role => new Claim(claimTypeRole, role.Name!)));

		// add claims
		userRoles.ToList().ForEach(role =>
		{
			authClaims.AddRange(role.Claims!.Select(claim => new Claim(claim.ClaimType!, claim.ClaimValue!)));
		});
		var userClaims = user.Claims ?? await identityRepo.GetClaimsAsync(user);
		authClaims.AddRange(userClaims.Select(claim => new Claim(claim.ClaimType!, claim.ClaimValue!)));

		// Add more claims as needed

		return jwtService.GenerateToken([.. authClaims], expiry);
	}

	private static async Task<BatUser?> FetchUserFirstNonEmpty(IIdentityRepository identityRepo, string? id, string? name, string? email)
	{
		// fetch user account in order: user-id, user-name, then user-email
		// feel free to change the order as needed
		return !string.IsNullOrEmpty(id)
			? await identityRepo.GetUserByIDAsync(id)
			: !string.IsNullOrEmpty(name)
				? await identityRepo.GetUserByUserNameAsync(name)
				: !string.IsNullOrEmpty(email)
					? await identityRepo.GetUserByEmailAsync(email)
					: null;
	}

	private static string? FirstNonEmpty(params string?[] values)
	{
		return values.FirstOrDefault(v => !string.IsNullOrEmpty(v)) ?? null;
	}

	/// <inheritdoc />
	public async Task<AuthResp> AuthenticateAsync(AuthReq req)
	{
		using (var scope = serviceProvider.CreateScope())
		{
			var identityRepo = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
			var user = await FetchUserFirstNonEmpty(identityRepo, req.Id, req.Name, req.Email);
			if (user == null)
			{
				var ustr = FirstNonEmpty(req.Id, req.Name, req.Email);
				logger.LogError("Authentication failed: user '{user}' not found.", ustr);
				return AuthResp.AuthFailed;
			}
			var pwdHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<BatUser>>();
			var pwdHashResult = pwdHasher.VerifyHashedPassword(user, user.PasswordHash!, req.Secret ?? req.Password ?? string.Empty);
			if (pwdHashResult != PasswordVerificationResult.Success && pwdHashResult != PasswordVerificationResult.SuccessRehashNeeded)
			{
				logger.LogError("Authentication failed: password verification failed.");
				return AuthResp.AuthFailed;
			}
			var expiry = DateTime.Now.AddSeconds(expirationSeconds);
			return AuthResp.New(200, await GenerateJwtToken(identityRepo, user, expiry), expiry);
		}
	}

	/// <inheritdoc />
	public AuthResp Authenticate(AuthReq req)
	{
		return AuthenticateAsync(req).Result;
	}

	/// <inheritdoc />
	public async Task<AuthResp> RefreshAsync(string jwtToken, bool ignoreTokenSecurityCheck = false)
	{
		try
		{
			var principal = jwtService.ValidateToken(jwtToken, out _);
			var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == claimTypeUserId)?.Value;
			var claimUserName = principal.Claims.FirstOrDefault(c => c.Type == claimTypeUserName)?.Value;
			var claimUserEmail = principal.Claims.FirstOrDefault(c => c.Type == claimTypeEmail)?.Value;
			using (var scope = serviceProvider.CreateScope())
			{
				var identityRepo = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
				var user = await FetchUserFirstNonEmpty(identityRepo, claimUserId, claimUserName, claimUserEmail);
				if (user == null)
				{
					var ustr = FirstNonEmpty(claimUserId, claimUserName, claimUserEmail);
					logger.LogError("AuthToken refreshing failed: user '{user}' not found.", ustr);
					return AuthResp.New(403, "User not found.");
				}
				if (!ignoreTokenSecurityCheck)
				{
					var claimSec = principal.Claims.FirstOrDefault(c => c.Type == claimTypeSecurity)?.Value ?? DefaultSecurityStampValue;
					if (claimSec != user.SecurityStamp?.Substring(user.SecurityStamp.Length - 8))
					{
						logger.LogError("AuthToken refreshing failed: invalid security stamp.");
						return AuthResp.New(403, "Invalid security stamp.");
					}
				}
				await identityRepo.UpdateSecurityStampAsync(user); // new token should invalidate all previous tokens
				var expiry = DateTime.Now.AddSeconds(expirationSeconds);
				return AuthResp.New(200, await GenerateJwtToken(identityRepo, user, expiry), expiry);
			}
		}
		catch (Exception e) when (e is ArgumentException || e is SecurityTokenException)
		{
			return AuthResp.New(403, e.Message);
		}
	}

	/// <inheritdoc />
	public AuthResp Refresh(string jwtToken, bool ignoreTokenSecurityCheck = false)
	{
		return RefreshAsync(jwtToken, ignoreTokenSecurityCheck).Result;
	}

	/// <inheritdoc />
	public async Task<TokenValidationResp> ValidateAsync(string jwtToken)
	{
		try
		{
			var principal = jwtService.ValidateToken(jwtToken, out _);
			var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == claimTypeUserId)?.Value;
			var claimUserName = principal.Claims.FirstOrDefault(c => c.Type == claimTypeUserName)?.Value;
			var claimUserEmail = principal.Claims.FirstOrDefault(c => c.Type == claimTypeEmail)?.Value;
			using (var scope = serviceProvider.CreateScope())
			{
				var identityRepo = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
				var user = await FetchUserFirstNonEmpty(identityRepo, claimUserId, claimUserName, claimUserEmail);
				if (user == null)
				{
					return new TokenValidationResp { Status = 404, Error = "User not found." };
				}
				var claimSec = principal.Claims.FirstOrDefault(c => c.Type == claimTypeSecurity)?.Value ?? DefaultSecurityStampValue;
				if (claimSec != user.SecurityStamp?.Substring(user.SecurityStamp.Length - 8))
				{
					return new TokenValidationResp { Status = 403, Error = "Invalid security stamp." };
				}
				return new TokenValidationResp { Status = 200, Principal = principal };
			}
		}
		catch (Exception e) when (e is ArgumentException || e is SecurityTokenException)
		{
			return new TokenValidationResp { Status = 403, Error = e.Message };
		}
	}

	/// <inheritdoc />
	public TokenValidationResp Validate(string jwtToken)
	{
		return ValidateAsync(jwtToken).Result;
	}
}
