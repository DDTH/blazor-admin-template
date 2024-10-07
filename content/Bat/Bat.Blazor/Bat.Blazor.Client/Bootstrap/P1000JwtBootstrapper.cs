using Bat.Shared.Bootstrap;
using Bat.Shared.Jwt;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Bat.Blazor.Client.Bootstrap;

/// <summary>
/// Bootstrapper that initializes a light-weighted JWT service used only in Blazor WASM mode.
/// </summary>
/// <remarks>
///		- The light-weighted JWT service is meant to quickly verify JWT tokens in Blazor WASM mode. Signing verification is ignored!
///		Client-side JWT verification is not secure and should not replace server-side verification.
///		- Bootstrappers in Blazor.Client project are invoked only in WebAssembly mode.
/// </remarks>
[Bootstrapper]
public class JwtBootstrapper
{
	public static void ConfigureWasmBuilder(WebAssemblyHostBuilder wasmAppBuilder)
	{
		Console.WriteLine("[INFO] Initializing JWT...");

		// store JWT configurations in the service container
		wasmAppBuilder.Services.Configure<JwtOptions>(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = false,
				ValidateTokenReplay = false,
				ValidateIssuer = false,
				ValidateAudience = false,
				ClockSkew = TimeSpan.Zero,
				SignatureValidator = (token, parameters) => new JwtSecurityToken(token) //ignore signature verification
			};
		});

		// register JwtService in the service container
		wasmAppBuilder.Services.AddSingleton<IJwtService, JwtService>();

		Console.WriteLine("[INFO] JWT initialized.");
	}
}
