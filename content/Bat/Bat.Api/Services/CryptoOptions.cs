using System.Security.Cryptography;

namespace Bat.Api.Services;

public sealed class CryptoOptions
{
	/// <summary>
	/// RSA public key for verifying API calls.
	/// </summary>
	public RSA RSAPrivKey { get; set; } = default!;

	/// <summary>
	/// RSA public key, derived from the private key.
	/// </summary>
	public RSA RSAPubKey { get; set; } = default!;
}
