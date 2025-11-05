using System.Security.Claims;
using Bat.Shared.Api;
using Bat.Shared.Global;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bat.Api.Controllers;

public partial class UsersController
{
	/// <summary>
	/// Gets all available roles.
	/// </summary>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_ROLES)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public async Task<ActionResult<ApiResp<IEnumerable<RoleResp>>>> GetAllRoles()
	{
		var roles = IdentityRepository.AllRolesAsync();
		var result = new List<RoleResp>();
		await foreach (var role in roles)
		{
			role.Claims ??= await IdentityRepository.GetClaimsAsync(role);
			result.Add(RoleResp.BuildFromRole(role));
		}
		return ResponseOk(result);
	}

	/// <summary>
	/// Gets a role by id.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[HttpGet(IApiClient.API_ENDPOINT_ROLES_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_USER_MANAGER)]
	public async Task<ActionResult<ApiResp<RoleResp>>> GetRole([FromRoute] string id)
	{
		var role = await IdentityRepository.GetRoleByIDAsync(id, RoleFetchOptions.DEFAULT.FetchClaims());
		if (role == null)
		{
			return ResponseNoData(404, $"Role '{id}' not found.");
		}
		role.Claims ??= await IdentityRepository.GetClaimsAsync(role);
		return ResponseOk(RoleResp.BuildFromRole(role));
	}

	/// <summary>
	/// Creates a new role.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="lookupNormalizer"></param>
	/// <returns></returns>
	/// <response code="200">Role created successfully.</response>
	/// <response code="400">Input validation failed (e.g. role already exists).</response>
	/// <response code="500">Failed to create role.</response>
	/// <response code="509">Failed to add claims to role, but role was created.</response>
	[HttpPost(IApiClient.API_ENDPOINT_ROLES)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_CREATE_ROLE_PERM)]
	public async Task<ActionResult<ApiResp<RoleResp>>> CreateRole(
		CreateOrUpdateRoleReq req,
		ILookupNormalizer lookupNormalizer)
	{
		var (authErrorResult, _) = await VerifyAuthTokenAndCurrentUser();
		if (authErrorResult != null)
		{
			// current auth token and signed-in user should all be valid
			return authErrorResult;
		}

		// validate the role name
		var roleName = req.Name?.Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(roleName))
		{
			return ResponseNoData(400, "Role name is required.");
		}

		// check if the role name is already taken
		var existingRole = await IdentityRepository.GetRoleByNameAsync(roleName);
		if (existingRole != null)
		{
			return ResponseNoData(400, $"Role '{roleName}' already exists.");
		}

		// verify if the claims are valid
		var uniqueClaims = (req.Claims?.Distinct() ?? []).Select(c => new Claim(c.Type, c.Value)).ToList();
		var invalidClaim = uniqueClaims.Where(c => !GlobalRegistry.ALL_CLAIMS.Contains(c, ClaimEqualityComparer.INSTANCE)).First();
		if (invalidClaim != null)
		{
			return ResponseNoData(400, $"Claim '{invalidClaim.Type}:{invalidClaim.Value}' is not valid.");
		}

		// first, create the role
		var role = new BatRole
		{
			Name = roleName,
			NormalizedName = lookupNormalizer.NormalizeName(roleName),
			Description = req.Description?.Trim() ?? string.Empty,
		};
		var iresult = await IdentityRepository.CreateAsync(role);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(500, $"Failed to create role: {iresult}");
		}

		// then add the claims
		iresult = await IdentityRepository.AddClaimsAsync(role, uniqueClaims);
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
	/// <param name="lookupNormalizer"></param>
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
		ILookupNormalizer lookupNormalizer)
	{
		var (authErrorResult, _) = await VerifyAuthTokenAndCurrentUser();
		if (authErrorResult != null)
		{
			// current auth token and signed-in user should all be valid
			return authErrorResult;
		}

		var targetRole = await IdentityRepository.GetRoleByIDAsync(id, RoleFetchOptions.DEFAULT.FetchClaims());
		if (targetRole == null)
		{
			return ResponseNoData(404, $"Role '{id}' not found.");
		}

		var roleName = req.Name?.Trim() ?? targetRole.Name; // if not provided, keep the original name
		if (!string.IsNullOrWhiteSpace(roleName))
		{
			var existingRole = await IdentityRepository.GetRoleByNameAsync(roleName);
			if (existingRole != null && !existingRole.Id.Equals(targetRole.Id, StringComparison.InvariantCulture))
			{
				return ResponseNoData(400, $"Role '{roleName}' already exists.");
			}
		}

		// verify if the claims are valid
		var uniqueClaimsNew = (req.Claims?.Distinct() ?? []).Select(c => new Claim(c.Type, c.Value)).ToList();
		var invalidClaim = uniqueClaimsNew.Where(c => !GlobalRegistry.ALL_CLAIMS.Contains(c, ClaimEqualityComparer.INSTANCE)).First();
		if (invalidClaim != null)
		{
			return ResponseNoData(400, $"Claim '{invalidClaim.Type}:{invalidClaim.Value}' is not valid.");
		}

		// first, update the role
		targetRole.Name = roleName;
		targetRole.NormalizedName = lookupNormalizer.NormalizeName(roleName);
		targetRole.Description = req.Description?.Trim() ?? targetRole.Description; // if not provided, keep the original description
		var iresultUpdate = await IdentityRepository.UpdateAsync(targetRole);
		if (iresultUpdate == null)
		{
			return ResponseNoData(500, $"Failed to update role.");
		}

		// then, update the claims, only if provided
		if (req.Claims is not null)
		{
			if (targetRole.Claims != null)
			{
				var iresultRemoveClaims = await IdentityRepository.RemoveClaimsAsync(targetRole, targetRole.Claims.Select(c => new Claim(c.ClaimType!, c.ClaimValue!)));
				if (!IIdentityRepository.IsSucceededOrNoChangesSaved(iresultRemoveClaims))
				{
					return ResponseNoData(509, $"Failed to update role's claims: {iresultRemoveClaims} / Note: Role's data was updated.");
				}
			}
			var iresultAddClaims = await IdentityRepository.AddClaimsAsync(targetRole, uniqueClaimsNew);
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
	/// <returns></returns>
	[HttpDelete(IApiClient.API_ENDPOINT_ROLES_ID)]
	[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_DELETE_ROLE_PERM)]
	public async Task<ActionResult<ApiResp<RoleResp>>> DeleteRole([FromRoute] string id)
	{
		var (authErrorResult, _) = await VerifyAuthTokenAndCurrentUser();
		if (authErrorResult != null)
		{
			// current auth token and signed-in user should all be valid
			return authErrorResult;
		}

		var role = await IdentityRepository.GetRoleByIDAsync(id);
		if (role == null)
		{
			return ResponseNoData(404, $"Role '{id}' not found.");
		}
		var iresult = await IdentityRepository.DeleteAsync(role);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(500, $"Failed to delete role: {iresult}");
		}
		return ResponseOk(RoleResp.BuildFromRole(role));
	}
}
