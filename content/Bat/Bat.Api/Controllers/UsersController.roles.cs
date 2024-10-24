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
	/// <response code="509">Failed to add claims to role, but role was created successfully.</response>
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

		var existingRole = await identityRepository.GetRoleByNameAsync(req.Name);
		if (existingRole != null)
		{
			return ResponseNoData(400, $"Role '{req.Name}' already exists.");
		}

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
			Name = req.Name.Trim(),
			NormalizedName = lookupNormalizer.NormalizeName(req.Name),
			Description = req.Description?.Trim(),
		};
		var iresult = await identityRepository.CreateAsync(role);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(500, $"Failed to create role: {iresult.ToString()}");
		}

		// then add the claims
		iresult = await identityRepository.AddClaimsAsync(role, claims);
		if (!iresult.Succeeded)
		{
			return ResponseNoData(509, $"Failed to add claims to role: {iresult.ToString()} / Note: Role was created successfully.");
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

		var role = await identityRepository.GetRoleByIDAsync(id, RoleFetchOptions.DEFAULT.FetchClaims());
		if (role == null)
		{
			return ResponseNoData(404, $"Role '{id}' not found.");
		}

		var existingRole = await identityRepository.GetRoleByNameAsync(req.Name);
		if (existingRole != null && !existingRole.Id.Equals(role.Id, StringComparison.InvariantCulture))
		{
			return ResponseNoData(400, $"Role '{req.Name}' already exists.");
		}

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

		// first, update the role
		role.Name = req.Name.Trim();
		role.NormalizedName = lookupNormalizer.NormalizeName(req.Name);
		role.Description = req.Description?.Trim();
		role = await identityRepository.UpdateAsync(role);
		if (role == null)
		{
			return ResponseNoData(500, $"Failed to update role.");
		}

		// then, update the claims
		if (role.Claims != null)
		{
			var iresultRemove = await identityRepository.RemoveClaimsAsync(role, role.Claims.Select(c => new Claim(c.ClaimType!, c.ClaimValue!)));
			if (!IIdentityRepository.IsSucceededOrNoChangesSaved(iresultRemove))
			{
				return ResponseNoData(500, $"Failed to update role's claims: {iresultRemove.ToString()}");
			}
		}
		var iresultAdd = await identityRepository.AddClaimsAsync(role, claims);
		if (!IIdentityRepository.IsSucceededOrNoChangesSaved(iresultAdd))
		{
			return ResponseNoData(500, $"Failed to update role's claims: {iresultAdd.ToString()}");
		}

		return ResponseOk(RoleResp.BuildFromRole(role));
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
			return ResponseNoData(500, $"Failed to delete role: {iresult.ToString()}");
		}
		return ResponseOk(RoleResp.BuildFromRole(role));
	}
}
