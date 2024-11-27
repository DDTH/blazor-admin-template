using System.Text.Json.Serialization;

namespace Bat.Shared.Api;

/// <summary>
/// Request info for the <see cref="IApiClient.LoginAsync(AuthReq, string?, HttpClient?, CancellationToken)"/> API call.
/// </summary>
public struct AuthReq
{
	/// <summary>
	/// Credentials: client/user id.
	/// </summary>
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Id { get; set; }

	/// <summary>
	/// Credentials: client/user name.
	/// </summary>
	[JsonPropertyName("name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Name { get; set; }

	/// <summary>
	/// Credentials: client/user email.
	/// </summary>
	[JsonPropertyName("email")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Email { get; set; }

	/// <summary>
	/// Credentials: client/user secret.
	/// </summary>
	[JsonPropertyName("secret")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Secret { get; set; }

	/// <summary>
	/// Credentials: client/user password.
	/// </summary>
	[JsonPropertyName("password")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Password { get; set; }

	/// <summary>
	/// (Optional) Encryption settings.
	/// </summary>
	[JsonPropertyName("encryption")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Encryption { get; set; }
}

/// <summary>
/// Response structure for APIs that return authentication information.
/// </summary>
public struct AuthResp
{
	public static readonly AuthResp AuthFailed = new() { Status = 403, Error = "Authentication failed." };
	public static readonly AuthResp TokenExpired = new() { Status = 401, Error = "Token expired." };

	/// <summary>
	/// Convenience method to create a new AuthResp instance.
	/// </summary>
	/// <param name="status"></param>
	/// <param name="error"></param>
	/// <returns></returns>
	public static AuthResp New(int status, string error)
	{
		return new AuthResp { Status = status, Error = error ?? "" };
	}

	/// <summary>
	/// Convenience method to create a new AuthResp instance.
	/// </summary>
	/// <param name="status"></param>
	/// <param name="token"></param>
	/// <param name="expiry"></param>
	/// <returns></returns>
	public static AuthResp New(int status, string token, DateTime? expiry)
	{
		return new AuthResp { Status = status, Token = token ?? "", Expiry = expiry };
	}

	/// <summary>
	/// Authentication status.
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

	/// <summary>
	/// Authentication token, if successful.
	/// </summary>
	[JsonPropertyName("token")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Token { get; set; }

	/// <summary>
	/// When the token expires.
	/// </summary>
	[JsonPropertyName("expiry")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public DateTime? Expiry { get; set; }
}
