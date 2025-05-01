using Bat.Shared.Api.Controller;
using Bat.Shared.Api.Services;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bat.Api.Controllers;

public partial class UsersController : ApiBaseController
{
	private readonly IAuthenticator? Authenticator;
	private readonly IAuthenticatorAsync? AuthenticatorAsync;
	private readonly IIdentityRepository IdentityRepository;
	private readonly IdentityOptions IdentityOptions;

	public UsersController(
		IIdentityRepository identityRepository,
		IOptions<IdentityOptions> identityOptions,
		IAuthenticator? authenticator,
		IAuthenticatorAsync? authenticatorAsync)
	{
		ArgumentNullException.ThrowIfNull(identityRepository, nameof(identityRepository));
		ArgumentNullException.ThrowIfNull(identityOptions, nameof(identityOptions));
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined defined.");
		}

		IdentityRepository = identityRepository;
		IdentityOptions = identityOptions.Value;
		Authenticator = authenticator;
		AuthenticatorAsync = authenticatorAsync;
	}

	private async Task<(ActionResult?, BatUser)> VerifyAuthTokenAndCurrentUser()
	{
		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(Authenticator, AuthenticatorAsync, jwtToken);
		if (tokenValidationResult.Status != 200)
		{
			// the auth token should still be valid
			return (ResponseNoData(403, tokenValidationResult.Error), null!);
		}

		var currentUser = await GetCurrentUserAsync(IdentityOptions, IdentityRepository);
		if (currentUser == null)
		{
			// should not happen
			return (_respAuthenticationRequired, null!);
		}

		return (null, currentUser);
	}
}
