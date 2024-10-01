using Microsoft.IdentityModel.Tokens;

namespace Bat.Api.Services;

public class JwtOptions
{
	/// <summary>
	/// The security key to sign and verify JWT tokens.
	/// </summary>
	public SecurityKey Key { get; set; } = default!;

	/// <summary>
	/// The algorithm to use for signing and verifying JWT tokens.
	/// </summary>
	public string Algorithm { get; set; } = default!;

	/// <summary>
	/// The issuer to use for JWT tokens.
	/// </summary>
	public string Issuer { get; set; } = default!;

	/// <summary>
	/// The audience to use for JWT tokens.
	/// </summary>
	public string Audience { get; set; } = default!;

	/// <summary>
	/// The default expiration time for JWT tokens in seconds.
	/// </summary>
	public int DefaultExpirationSeconds { get; set; } = 3600;

	public TokenValidationParameters TokenValidationParameters { get; set; } = default!;
}
