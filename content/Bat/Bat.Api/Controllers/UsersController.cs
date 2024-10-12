using Bat.Api.Services;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
namespace Bat.Api.Controllers;

public partial class UsersController : ApiBaseController
{
	private async Task<(ActionResult?, BatUser)> VerifyAuthTokenAndCurrentUser(
	   IIdentityRepository identityRepository,
	   IdentityOptions identityOptions,
	   IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync)
	{
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.");
		}

		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(authenticator, authenticatorAsync, jwtToken!);
		Console.WriteLine($"VerifyAuthTokenAndCurrentUser/ValidateAuthTokenAsync: {JsonSerializer.Serialize(tokenValidationResult)}");
		if (tokenValidationResult.Status != 200)
		{
			// the auth token should still be valid
			return (ResponseNoData(403, tokenValidationResult.Error), null!);
		}

		var currentUser = await GetCurrentUserAsync(identityOptions, identityRepository);
		if (currentUser == null)
		{
			// should not happen  
			return (_respAuthenticationRequired, null!);
		}

		return (null, currentUser);
	}
}
