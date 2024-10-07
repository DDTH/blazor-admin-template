using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Bat.Shared.Jwt;

public interface IJwtService
{
	/// <summary>
	/// Generates a JWT token with default expiration.
	/// </summary>
	/// <param name="claims"></param>
	/// <returns></returns>
	public string GenerateToken(Claim[] claims);

	/// <summary>
	/// Generates a JWT token with specified expiration.
	/// </summary>
	/// <param name="claims"></param>
	/// <param name="expiry"></param>
	/// <returns></returns>
	public string GenerateToken(Claim[] claims, DateTime? expiry);

	/// <summary>
	/// Generate a JWT token with default expiration.
	/// </summary>
	/// <param name="subject"></param>
	/// <returns></returns>
	public string GenerateToken(ClaimsIdentity subject);

	/// <summary>
	/// Generates a JWT token with specified expiration.
	/// </summary>
	/// <param name="subject"></param>
	/// <param name="expiry"></param>
	/// <returns></returns>
	public string GenerateToken(ClaimsIdentity subject, DateTime? expiry);

	/// <summary>
	/// Validates a JWT token using the default validation parameters.
	/// </summary>
	/// <param name="token"></param>
	/// <param name="validatedToken"></param>
	/// <returns></returns>
	public ClaimsPrincipal ValidateToken(string token, out SecurityToken validatedToken);

	/// <summary>
	/// Validates a JWT token using the specified validation parameters.
	/// </summary>
	/// <param name="token"></param>
	/// <param name="validationParameters"></param>
	/// <param name="validatedToken"></param>
	/// <returns></returns>
	public ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken);
}
