using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bat.Shared.Jwt;

public class JwtService : IJwtService
{
	private readonly JwtOptions _options;

	public JwtService(IOptions<JwtOptions> jwtOptions)
	{
		ArgumentNullException.ThrowIfNull(jwtOptions, nameof(jwtOptions));

		_options = jwtOptions.Value;
	}

	/// <inheritdoc />
	public string GenerateToken(Claim[] claims)
	{
		return GenerateToken(new ClaimsIdentity(claims));
	}

	/// <inheritdoc />
	public string GenerateToken(Claim[] claims, DateTime? expiry)
	{
		return GenerateToken(new ClaimsIdentity(claims), expiry);
	}

	/// <inheritdoc />
	public string GenerateToken(ClaimsIdentity subject)
	{
		return GenerateToken(subject, DateTime.Now.AddSeconds(_options.DefaultExpirationSeconds));
	}

	/// <inheritdoc />
	public string GenerateToken(ClaimsIdentity subject, DateTime? expiry)
	{
		var token = new JwtSecurityToken(
			issuer: _options.Issuer,
			audience: _options.Audience,
			claims: subject.Claims,
			expires: expiry?.ToUniversalTime() ?? DateTime.UtcNow.AddSeconds(_options.DefaultExpirationSeconds),
			signingCredentials: new SigningCredentials(_options.Key, _options.Algorithm)
		);
		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	/// <inheritdoc />
	public ClaimsPrincipal ValidateToken(string token)
	{
		return ValidateToken(token, _options.TokenValidationParameters);
	}

	/// <inheritdoc />
	public ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		return tokenHandler.ValidateToken(token, validationParameters, out _);
	}
}
