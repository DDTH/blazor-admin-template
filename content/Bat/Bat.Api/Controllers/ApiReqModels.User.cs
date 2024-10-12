using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Bat.Api.Controllers;

/// <summary>
/// Request to update user profile.
/// </summary>
public struct UpdateUserProfileReq
{
	[BindProperty(Name = "given_name")]
	[JsonPropertyName("given_name")]
	public string? GivenName { get; set; }

	[BindProperty(Name = "family_name")]
	[JsonPropertyName("family_name")]
	public string? FamilyName { get; set; }
}

/// <summary>
/// Request to change user password.
/// </summary>
public struct ChangePasswordReq
{
	[BindProperty(Name = "current_password")]
	[JsonPropertyName("current_password")]
	public string CurrentPassword { get; set; }

	[BindProperty(Name = "new_password")]
	[JsonPropertyName("new_password")]
	public string NewPassword { get; set; }
}
