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
	/// <response code="200">User created successfully.</response>
	/// <response code="400">Input validation failed (e.g. user already exists).</response>
	/// <response code="500">Failed to create user.</response>
	/// <response code="509">Failed to add claims to user or user to roles, but user was created.</response>
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

		// validate the username
		var username = req.Username?.ToLower().Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(username))
		{
			return ResponseNoData(400, "Username is required.");
		}

		// validate the email
		var email = req.Email?.ToLower().Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(email))
		{
			return ResponseNoData(400, "Email is required.");
		}

		// validate the password
		var password = req.Password?.Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(password))
		{
			return ResponseNoData(400, "Password is required.");
		}
		var vPasswordResult = await passwordValidator.ValidateAsync(userManager, null!, password);
		if (vPasswordResult != IdentityResult.Success)
		{
			return ResponseNoData(400, vPasswordResult.ToString());
		}

		// check if the username is already taken
		var existingUserName = await identityRepository.GetUserByUserNameAsync(username);
		if (existingUserName != null)
		{
			return ResponseNoData(400, $"User '{username}' already exists.");
		}

		// check if the email is already taken
		var existingUserEmail = await identityRepository.GetUserByEmailAsync(email);
		if (existingUserEmail != null)
		{
			return ResponseNoData(400, $"Email '{email}' has been used by another user.");
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
			UserName = username,
			NormalizedUserName = lookupNormalizer.NormalizeName(username),
			Email = email,
			NormalizedEmail = lookupNormalizer.NormalizeEmail(email),
			PasswordHash = passwordHasher.HashPassword(null!, password),
			FamilyName = req.FamilyName?.Trim(),
			GivenName = req.GivenName?.Trim(),
		};
		var iresultCreate = await identityRepository.CreateAsync(user);
		if (!iresultCreate.Succeeded)
		{
			return ResponseNoData(500, $"Failed to create user: {iresultCreate.ToString()}");
		}

		// then add the claims
		var iresultAddClaims = await identityRepository.AddClaimsAsync(user, claims);
		if (!iresultAddClaims.Succeeded)
		{
			return ResponseNoData(509, $"Failed to add claims to user: {iresultAddClaims.ToString()} / Note: User has been created.");
		}

		// then add the roles
		var iresultAddRoles = await identityRepository.AddToRolesAsync(user, roles);
		if (!iresultAddRoles.Succeeded)
		{
			return ResponseNoData(509, $"Failed to add roles to user: {iresultAddRoles.ToString()} / Note: User has been created.");
		}

		return ResponseOk(UserResp.BuildFromUser(user));
	}

	/// <summary>
	/// Updates an existing user.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="req"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="lookupNormalizer"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	/// <response code="200">User updated successfully.</response>
	/// <response code="400">Input validation failed (e.g. user's name already used by another one).</response>
	/// <response code="404">User not found.</response>
	/// <response code="500">Failed to update user.</response>
	/// <response code="509">Failed to update user's roles or claims, but other user data was updated.</response>
	[HttpPut(IApiClient.API_ENDPOINT_USERS_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_USER_PERM)]
	public async Task<ActionResult<ApiResp<UserResp>>> UpdateUser(
		[FromRoute] string id,
		CreateOrUpdateUserReq req,
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository,
		ILookupNormalizer lookupNormalizer,
		IAuthenticator? authenticator,
		IAuthenticatorAsync? authenticatorAsync)
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

		var targetUser = await identityRepository.GetUserByIDAsync(id, UserFetchOptions.DEFAULT.FetchRoles().FetchClaims());
		if (targetUser == null)
		{
			return ResponseNoData(404, $"User '{id}' not found.");
		}

		var username = req.Username?.ToLower().Trim() ?? targetUser.UserName; // if not provided, keep the original username
		if (!string.IsNullOrWhiteSpace(username))
		{
			var existingUserName = await identityRepository.GetUserByUserNameAsync(username);
			if (existingUserName != null && !existingUserName.Id.Equals(targetUser.Id, StringComparison.InvariantCulture))
			{
				return ResponseNoData(400, $"Username '{username}' already exists.");
			}
		}

		var email = req.Email?.ToLower().Trim() ?? targetUser.Email; // if not provided, keep the original email
		if (!string.IsNullOrWhiteSpace(email))
		{
			var existingUserEmail = await identityRepository.GetUserByEmailAsync(email);
			if (existingUserEmail != null && !existingUserEmail.Id.Equals(targetUser.Id, StringComparison.InvariantCulture))
			{
				return ResponseNoData(400, $"Email '{email}' has been used by another user.");
			}
		}

		// verify if the claims are valid
		var uniqueClaimsNew = req.Claims?.Distinct() ?? [];
		var claimsNew = new List<Claim>();
		foreach (var uc in uniqueClaimsNew)
		{
			var claim = new Claim(uc.Type, uc.Value);
			if (!BuiltinClaims.ALL_CLAIMS.Contains(claim, ClaimEqualityComparer.INSTANCE))
			{
				return ResponseNoData(400, $"Claim '{claim.Type}:{claim.Value}' is not valid.");
			}
			claimsNew.Add(claim);
		}

		// verify if the roles are valid
		var uniqueRolesNew = req.Roles?.Distinct() ?? [];
		var rolesNew = new List<BatRole>();
		foreach (var ur in uniqueRolesNew)
		{
			var role = await identityRepository.GetRoleByIDAsync(ur);
			if (role == null)
			{
				return ResponseNoData(400, $"Role '{ur}' does not exist.");
			}
			rolesNew.Add(role);
		}

		// first, update the user
		targetUser.UserName = username;
		targetUser.NormalizedUserName = lookupNormalizer.NormalizeName(username);
		targetUser.Email = email;
		targetUser.NormalizedEmail = lookupNormalizer.NormalizeEmail(email);
		targetUser.FamilyName = req.FamilyName?.Trim() ?? targetUser.FamilyName; // if not provided, keep the original family name
		targetUser.GivenName = req.GivenName?.Trim() ?? targetUser.GivenName; // if not provided, keep the original given name
		var iresultUpdate	 = await identityRepository.UpdateAsync(targetUser);
		if (iresultUpdate == null)
		{
			return ResponseNoData(500, $"Failed to update user.");
		}

		// then, update the roles, only if provided
		if (req.Roles is not null)
		{
			if (targetUser.Roles != null)
			{
				var iresultRemoveRoles = await identityRepository.RemoveFromRolesAsync(targetUser, targetUser.Roles);
				if (!iresultRemoveRoles.Succeeded)
				{
					return ResponseNoData(509, $"Failed to update user's roles: {iresultRemoveRoles.ToString()} / Note: User's data was updated.");
				}
			}
			var iresultAddRoles = await identityRepository.AddToRolesAsync(targetUser, rolesNew);
			if (!iresultAddRoles.Succeeded)
			{
				return ResponseNoData(509, $"Failed to update user's roles: {iresultAddRoles.ToString()} / Note: User's data was updated.");
			}
		}

		// finally, update the claims, only if provided
		if (req.Claims is not null)
		{
			if (targetUser.Claims != null)
			{
				var iresultRemoveClaims = await identityRepository.RemoveClaimsAsync(targetUser, targetUser.Claims.Select(c => new Claim(c.ClaimType!, c.ClaimValue!)));
				if (!IIdentityRepository.IsSucceededOrNoChangesSaved(iresultRemoveClaims))
				{
					return ResponseNoData(509, $"Failed to update user's claims: {iresultRemoveClaims.ToString()} / Note: User's data was updated.");
				}
			}
			var iresultAddClaims = await identityRepository.AddClaimsAsync(targetUser, claimsNew);
			if (!IIdentityRepository.IsSucceededOrNoChangesSaved(iresultAddClaims))
			{
				return ResponseNoData(509, $"Failed to update user's claims: {iresultAddClaims.ToString()} / Note: User's data was updated.");
			}
		}

		return ResponseOk(UserResp.BuildFromUser(targetUser));
	}

	/// <summary>
	/// Deletes a user by id.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	[HttpDelete(IApiClient.API_ENDPOINT_USERS_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_USER_PERM)]
	public async Task<ActionResult<ApiResp<UserResp>>> DeleteUser(
		[FromRoute] string id,
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository,
		IAuthenticator? authenticator,
		IAuthenticatorAsync? authenticatorAsync)
	{
		var (vAuthTokenResult, currentUser) = await VerifyAuthTokenAndCurrentUser(
			identityRepository,
			identityOptions.Value,
			authenticator, authenticatorAsync);
		if (vAuthTokenResult != null)
		{
			// current auth token and signed-in user should all be valid
			return vAuthTokenResult;
		}

		var user = await identityRepository.GetUserByIDAsync(id);
		if (user == null)
		{
			return ResponseNoData(404, $"User '{id}' not found.");
		}
		if (user.Id.Equals(currentUser.Id, StringComparison.InvariantCulture))
		{
			return ResponseNoData(400, "You cannot delete yourself.");
		}
		var iresult = await identityRepository.DeleteAsync(user);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(500, $"Failed to delete user: {iresult.ToString()}");
		}
		return ResponseOk(UserResp.BuildFromUser(user));
	}
}
