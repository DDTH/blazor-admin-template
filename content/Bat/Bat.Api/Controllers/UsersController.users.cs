using System.Security.Claims;
using Bat.Api.Services;
using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bat.Api.Controllers;

public partial class UsersController
{
	/// <summary>
	/// Gets all available roles.
	/// </summary>
	/// <param name="identityRepository"></param>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_USERS)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public async Task<ActionResult<ApiResp<IEnumerable<UserResp>>>> GetAllUsers(IIdentityRepository identityRepository)
	{
		var users = identityRepository.AllUsersAsync();
		var result = new List<UserResp>();
		await foreach (var user in users)
		{
			user.Roles ??= await identityRepository.GetRolesAsync(user);
			user.Claims ??= await identityRepository.GetClaimsAsync(user);
			result.Add(UserResp.BuildFromUser(user));
		}
		return ResponseOk(result);
	}

	/// <summary>
	/// Gets a user by id.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="identityRepository"></param>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_USERS_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public async Task<ActionResult<ApiResp<UserResp>>> GetUser([FromRoute] string id, IIdentityRepository identityRepository)
	{
		var user = await identityRepository.GetUserByIDAsync(id, UserFetchOptions.DEFAULT.FetchRoles().FetchClaims());
		if (user == null)
		{
			return ResponseNoData(404, $"User '{id}' not found.");
		}
		user.Roles ??= await identityRepository.GetRolesAsync(user);
		user.Claims ??= await identityRepository.GetClaimsAsync(user);
		return ResponseOk(UserResp.BuildFromUser(user));
	}

	/// <summary>
	/// Creates a new user.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="lookupNormalizer"></param>
	/// <param name="passwordValidator"></param>
	/// <param name="passwordHasher"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <param name="userManager"></param>
	/// <returns></returns>
	/// <response code="200">Role created successfully.</response>
	/// <response code="400">Input validation failed (e.g. user already exists).</response>
	/// <response code="500">Failed to create user.</response>
	/// <response code="509">Failed to add claims to user or user to roles, but user was created successfully.</response>
	[HttpPost(IApiClient.API_ENDPOINT_USERS)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_USER_PERM)]
	public async Task<ActionResult<ApiResp<UserResp>>> CreateUser(
		CreateOrUpdateUserReq req,
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository,
		ILookupNormalizer lookupNormalizer,
		IPasswordValidator<BatUser> passwordValidator,
		IPasswordHasher<BatUser> passwordHasher,
		IAuthenticator? authenticator,
		IAuthenticatorAsync? authenticatorAsync,
		UserManager<BatUser> userManager)
	{
		var (vAuthTokenResult, _) = await VerifyAuthTokenAndCurrentUser(
			identityRepository,
			identityOptions.Value,
			authenticator, authenticatorAsync);
		if (vAuthTokenResult != null)
		{
			// current auth token and signed-in user should all be valid
			return vAuthTokenResult;
		}

		var existingUserName = await identityRepository.GetUserByUserNameAsync(req.Username);
		if (existingUserName != null)
		{
			return ResponseNoData(400, $"User '{req.Username}' already exists.");
		}

		var existingUserEmail = await identityRepository.GetUserByEmailAsync(req.Email);
		if (existingUserEmail != null)
		{
			return ResponseNoData(400, $"Email '{req.Email}' has been used by another user.");
		}

		// verify if the password meets the compexity requirements
		var vPasswordResult = await passwordValidator.ValidateAsync(userManager, null!, req.Password);
		if (vPasswordResult != IdentityResult.Success)
		{
			return ResponseNoData(400, vPasswordResult.ToString());
		}

		// verify if the claims are valid
		var uniqueClaims = req.Claims?.Distinct() ?? [];
		var claims = new List<Claim>();
		foreach (var uc in uniqueClaims)
		{
			var claim = new Claim(uc.Type, uc.Value);
			if (!BuiltinClaims.ALL_CLAIMS.Contains(claim, ClaimEqualityComparer.INSTANCE))
			{
				return ResponseNoData(400, $"Claim '{claim.Type}:{claim.Value}' is not valid.");
			}
			claims.Add(claim);
		}

		// verify if the roles are valid
		var uniqueRoles = req.Roles?.Distinct() ?? [];
		var roles = new List<BatRole>();
		foreach (var ur in uniqueRoles)
		{
			var role = await identityRepository.GetRoleByIDAsync(ur);
			if (role == null)
			{
				return ResponseNoData(400, $"Role '{ur}' does not exist.");
			}
			roles.Add(role);
		}

		// first, create the user
		var user = new BatUser
		{
			UserName = req.Username.Trim(),
			NormalizedUserName = lookupNormalizer.NormalizeName(req.Username),
			Email = req.Email.Trim(),
			NormalizedEmail = lookupNormalizer.NormalizeEmail(req.Email),
			PasswordHash = passwordHasher.HashPassword(null!, req.Password),
			FamilyName = req.FamilyName?.Trim(),
			GivenName = req.GivenName?.Trim(),
		};
		var iresult = await identityRepository.CreateAsync(user);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(500, $"Failed to create user: {iresult.ToString()}");
		}

		// then add the claims
		iresult = await identityRepository.AddClaimsAsync(user, claims);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(509, $"Failed to add claims to user: {iresult.ToString()} / Note: User was created successfully.");
		}

		// then add the roles
		iresult = await identityRepository.AddToRolesAsync(user, roles);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(509, $"Failed to add roles to user: {iresult.ToString()} / Note: User was created successfully.");
		}

		return ResponseOk(UserResp.BuildFromUser(user));
	}
}