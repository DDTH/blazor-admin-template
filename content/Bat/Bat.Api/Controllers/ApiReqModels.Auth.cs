using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Bat.Api.Controllers;

/// <summary>
/// Request to authenticate.
/// </summary>
public struct AuthReq
{
	/// <summary>
	/// Credentials: client/user id.
	/// </summary>
	[BindProperty(Name = "id")]
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>
	/// Credentials: client/user name.
	/// </summary>
	[BindProperty(Name = "name")]
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>
	/// Credentials: client/user email.
	/// </summary>
	[BindProperty(Name = "email")]
	[JsonPropertyName("email")]
	public string? Email { get; set; }

	/// <summary>
	/// Credentials: client/user secret.
	/// </summary>
	[BindProperty(Name = "secret")]
	[JsonPropertyName("secret")]
	public string? Secret { get; set; }

	/// <summary>
	/// Credentials: client/user password.
	/// </summary>
	[BindProperty(Name = "password")]
	[JsonPropertyName("password")]
	public string? Password { get; set; }

	/// <summary>
	/// (Optional) Encryption settings.
	/// </summary>
	[BindProperty(Name = "encryption")]
	[JsonPropertyName("encryption")]
	public string? Encryption { get; set; }
}
