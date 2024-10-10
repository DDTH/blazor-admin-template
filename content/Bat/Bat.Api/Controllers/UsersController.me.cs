using Bat.Shared.Api;
using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Bat.Api.Controllers;

public partial class UsersController
{
	/// <summary>
	/// Returns current user's information.
	/// </summary>
	/// <returns></returns>
	[HttpGet("/api/users/-me")]
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResp>>> GetMyInfo(
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository)
	{
		var currentUser = await GetCurrentUserAsync(identityOptions.Value, identityRepository);
		if (currentUser == null)
		{
			return _respAuthenticationRequired;
		}
		Console.WriteLine($"User: {JsonSerializer.Serialize(currentUser)}");
		var userResponse = new UserResp
		{
			Id = currentUser.Id,
			Username = currentUser.UserName!,
			Email = currentUser.Email!,
			GivenName = currentUser.GivenName,
			FamilyName = currentUser.FamilyName,
			Roles = (await identityRepository.GetRolesAsync(currentUser)).Select(r => r.Name!)
		};
		return ResponseOk(userResponse);
	}
}
