using Bat.Api.Controllers;
using Bat.Shared.Api;

namespace Bat.Api.Services;

public interface IAuthenticator
{
	/// <summary>
	/// Perform an authentication action.
	/// </summary>
	/// <param name="req">The authentication request.</param>
	/// <returns></returns>
	public AuthResp Authenticate(AuthReq req);

	/// <summary>
	/// Refresh an issued authentication token.
	/// </summary>
	/// <param name="token">The issued authentication token.</param>
	/// <param name="ignoreTokenSecurityCheck">If true, donot check if token's security tag is still valid.</param>
	/// <returns></returns>
	public AuthResp Refresh(string token, bool ignoreTokenSecurityCheck = false);

	/// <summary>
	/// Validate an issued authentication token.
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	public TokenValidationResp Validate(string token);
}

public interface IAuthenticatorAsync
{
	/// <summary>
	/// Perform an authentication action.
	/// </summary>
	/// <param name="req">The authentication request.</param>
	/// <returns></returns>
	public Task<AuthResp> AuthenticateAsync(AuthReq req);

	/// <summary>
	/// Refresh an issued authentication token.
	/// </summary>
	/// <param name="token">The issued authentication token.</param>
	/// <param name="ignoreTokenSecurityCheck">If true, donot check if token's security tag is still valid.</param>
	/// <returns></returns>
	public Task<AuthResp> RefreshAsync(string token, bool ignoreTokenSecurityCheck = false);

	/// <summary>
	/// Validate an issued authentication token.
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	public Task<TokenValidationResp> ValidateAsync(string token);
}
