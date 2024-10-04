using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Bat.Api.Services;

/// <summary>
/// Resposne to a token validation request.
/// </summary>
public class TokenValidationResp
{
	/// <summary>
	/// Validation status.
	/// </summary>
	/// <value>200: success</value>
	[JsonIgnore]
	public int Status { get; set; }

	/// <summary>
	/// Additional error information, if any.
	/// </summary>
	[JsonPropertyName("error")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Error { get; set; }

	[JsonIgnore]
	public ClaimsPrincipal? Principal { get; set; }
}

/// <summary>
/// Authentication request.
/// </summary>
public class AuthReq
{
	/// <summary>
	/// Credentials: client/user id.
	/// </summary>
	[BindProperty(Name = "id")]
	public string? Id { get; set; }

	/// <summary>
	/// Credentials: client/user name.
	/// </summary>
	[BindProperty(Name = "name")]
	public string? Name { get; set; }

	/// <summary>
	/// Credentials: client/user email.
	/// </summary>
	[BindProperty(Name = "email")]
	public string? Email { get; set; }

	/// <summary>
	/// Credentials: client/user secret.
	/// </summary>
	[BindProperty(Name = "secret")]
	public string? Secret { get; set; }

	/// <summary>
	/// Credentials: client/user password.
	/// </summary>
	[BindProperty(Name = "password")]
	public string? Password { get; set; }

	/// <summary>
	/// (Optional) Encryption settings.
	/// </summary>
	[BindProperty(Name = "encryption")]
	public string? Encryption { get; set; }
}
