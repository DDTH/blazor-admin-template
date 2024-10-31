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
	[HttpGet(IApiClient.API_ENDPOINT_ROLES)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public async Task<ActionResult<ApiResp<IEnumerable<RoleResp>>>> GetAllRoles(IIdentityRepository identityRepository)
	{
		var roles = identityRepository.AllRolesAsync();
		var result = new List<RoleResp>();
		await foreach (var role in roles)
		{
			role.Claims ??= await identityRepository.GetClaimsAsync(role);
			result.Add(RoleResp.BuildFromRole(role));
		}
		return ResponseOk(result);
	}

	/// <summary>
	/// Gets a role by id.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="identityRepository"></param>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_ROLES_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public async Task<ActionResult<ApiResp<RoleResp>>> GetRole([FromRoute] string id, IIdentityRepository identityRepository)
	{
		var role = await identityRepository.GetRoleByIDAsync(id, RoleFetchOptions.DEFAULT.FetchClaims());
		if (role == null)
		{
			return ResponseNoData(404, $"Role '{id}' not found.");
		}
		role.Claims ??= await identityRepository.GetClaimsAsync(role);
		return ResponseOk(RoleResp.BuildFromRole(role));
	}

	/// <summary>
	/// Creates a new role.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="lookupNormalizer"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	/// <response code="200">Role created successfully.</response>
	/// <response code="400">Input validation failed (e.g. role already exists).</response>
	/// <response code="500">Failed to create role.</response>
	/// <response code="509">Failed to add claims to role, but role was created.</response>
	[HttpPost(IApiClient.API_ENDPOINT_ROLES)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_ROLE_PERM)]
	public async Task<ActionResult<ApiResp<RoleResp>>> CreateRole(
		CreateOrUpdateRoleReq req,
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

		// validate the role name
		var roleName = req.Name?.Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(roleName))
		{
			return ResponseNoData(400, "Role name is required.");
		}

		// check if the role name is already taken
		var existingRole = await identityRepository.GetRoleByNameAsync(roleName);
		if (existingRole != null)
		{
			return ResponseNoData(400, $"Role '{roleName}' already exists.");
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

		// first, create the role
		var role = new BatRole
		{
			Name = roleName,
			NormalizedName = lookupNormalizer.NormalizeName(roleName),
			Description = req.Description?.Trim() ?? string.Empty,
		};
		var iresult = await identityRepository.CreateAsync(role);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(500, $"Failed to create role: {iresult}");
		}

		// then add the claims
		iresult = await identityRepository.AddClaimsAsync(role, claims);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(509, $"Failed to add claims to role: {iresult} / Note: Role has been created.");
		}

		return ResponseOk(RoleResp.BuildFromRole(role));
	}

	/// <summary>
	/// Updates an existing role.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="req"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="lookupNormalizer"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	/// <response code="200">Role updated successfully.</response>
	/// <response code="400">Input validation failed (e.g. role's name already used by another one).</response>
	/// <response code="404">Role not found.</response>
	/// <response code="500">Failed to update role.</response>
	/// <response code="509">Failed to update role's claims, but other role data was updated.</response>
	[HttpPut(IApiClient.API_ENDPOINT_ROLES_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_MODIFY_ROLE_PERM)]
	public async Task<ActionResult<ApiResp<RoleResp>>> UpdateRole(
		[FromRoute] string id,
		CreateOrUpdateRoleReq req,
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

		var targetRole = await identityRepository.GetRoleByIDAsync(id, RoleFetchOptions.DEFAULT.FetchClaims());
		if (targetRole == null)
		{
			return ResponseNoData(404, $"Role '{id}' not found.");
		}

		var roleName = req.Name?.Trim() ?? targetRole.Name; // if not provided, keep the original name
		if (!string.IsNullOrWhiteSpace(roleName))
		{
			var existingRole = await identityRepository.GetRoleByNameAsync(roleName);
			if (existingRole != null && !existingRole.Id.Equals(targetRole.Id, StringComparison.InvariantCulture))
			{
				return ResponseNoData(400, $"Role '{roleName}' already exists.");
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

		// first, update the role
		targetRole.Name = roleName;
		targetRole.NormalizedName = lookupNormalizer.NormalizeName(roleName);
		targetRole.Description = req.Description?.Trim() ?? targetRole.Description; // if not provided, keep the original description
		var iresultUpdate = await identityRepository.UpdateAsync(targetRole);
		if (iresultUpdate == null)
		{
			return ResponseNoData(500, $"Failed to update role.");
		}

		// then, update the claims, only if provided
		if (req.Claims is not null)
		{
			if (targetRole.Claims != null)
			{
				var iresultRemoveClaims = await identityRepository.RemoveClaimsAsync(targetRole, targetRole.Claims.Select(c => new Claim(c.ClaimType!, c.ClaimValue!)));
				if (!IIdentityRepository.IsSucceededOrNoChangesSaved(iresultRemoveClaims))
				{
					return ResponseNoData(509, $"Failed to update role's claims: {iresultRemoveClaims} / Note: Role's data was updated.");
				}
			}
			var iresultAddClaims = await identityRepository.AddClaimsAsync(targetRole, claimsNew);
			if (!IIdentityRepository.IsSucceededOrNoChangesSaved(iresultAddClaims))
			{
				return ResponseNoData(509, $"Failed to update role's claims: {iresultAddClaims} / Note: Role's data was updated.");
			}
		}

		return ResponseOk(RoleResp.BuildFromRole(targetRole));
	}

	/// <summary>
	/// Deletes a role by id.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	[HttpDelete(IApiClient.API_ENDPOINT_ROLES_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_ROLE_PERM)]
	public async Task<ActionResult<ApiResp<RoleResp>>> DeleteRole(
		[FromRoute] string id,
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository,
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

		var role = await identityRepository.GetRoleByIDAsync(id);
		if (role == null)
		{
			return ResponseNoData(404, $"Role '{id}' not found.");
		}
		var iresult = await identityRepository.DeleteAsync(role);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(500, $"Failed to delete role: {iresult}");
		}
		return ResponseOk(RoleResp.BuildFromRole(role));
	}
}
