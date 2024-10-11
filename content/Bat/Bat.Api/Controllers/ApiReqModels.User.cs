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
