using Bat.Shared.Identity;
using Bat.Shared.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Bat.Shared.Api;

/// <summary>
/// Response to an API call.
/// </summary>
public class ApiResp
{
	/// <summary>
	/// Status of the API call, following HTTP status codes convention.
	/// </summary>
	[JsonPropertyName("status")]
	public int Status { get; set; }

	/// <summary>
	/// Extra information if any (e.g. the detailed error message).
	/// </summary>
	[JsonPropertyName("message")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Message { get; set; }

	/// <summary>
	/// Extra data if any.
	/// </summary>
	[JsonPropertyName("extras")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public object? Extras { get; set; }

	/// <summary>
	/// Debug information if any.
	/// </summary>
	[JsonPropertyName("debug_info")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public object? DebugInfo { get; set; }
}

/// <summary>
/// Typed version of <see cref="ApiResp"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ApiResp<T> : ApiResp
{
	/// <summary>
	/// The data returned by the API call (specific to individual API).
	/// </summary>
	[JsonPropertyName("data")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public T? Data { get; set; }
}

/*----------------------------------------------------------------------*/

/// <summary>
/// Response to the <<c>/info</c>> API call.
/// </summary>
public sealed class InfoResp
{
	[JsonPropertyName("ready")]
	public bool Ready { get; set; }

	[JsonPropertyName("app")]
	public AppInfo? App { get; set; }

	[JsonPropertyName("server")]
	public ServerInfo? Server { get; set; }

	[JsonPropertyName("crypto")]
	public CryptoInfo? Crypto { get; set; }
}

public sealed class CryptoInfo
{
	[JsonPropertyName("pub_key")]
	public string? PubKey { get; set; }
	[JsonPropertyName("pub_key_type")]
	public string? PubKeyType { get; set; }
}

public sealed class ServerInfo
{
	[JsonPropertyName("env")]
	public string? Env { get; set; }
	[JsonPropertyName("time")]
	public DateTime Time { get; set; } = DateTime.Now;
}

public sealed class AppInfo
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }
	[JsonPropertyName("version")]
	public string? Version { get; set; }
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}

/*----------------------------------------------------------------------*/

/// <summary>
/// Response to the <c>/auth/signin</c>, <c>/auth/login</c> or <c>/auth/refresh</c> API call.
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

	//private int _status;
	//private string? _error;
	//private string? _token;
	//private DateTime? _expiry;

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
	public string? Token { get; set; }

	/// <summary>
	/// When the token expires.
	/// </summary>
	[JsonPropertyName("expiry")]
	public DateTime? Expiry { get; set; }
}

/*----------------------------------------------------------------------*/

public struct ClaimResp
{
	[JsonPropertyName("type")]
	public string ClaimType { get; set; }

	[JsonPropertyName("value")]
	public string ClaimValue { get; set; }
}

/*----------------------------------------------------------------------*/

public struct UserResp
{
	public static UserResp BuildFromUser(BatUser user)
	{
		return new UserResp
		{
			Id = user.Id,
			Username = user.UserName!,
			Email = user.Email!,
			GivenName = user.GivenName,
			FamilyName = user.FamilyName,
			Roles = user.Roles?.Select(r => RoleResp.BuildFromRole(r)),
		};
	}

	public string Id { get; set; }
	public string Username { get; set; }
	public string Email { get; set; }

	[JsonPropertyName("given_name")]
	public string? GivenName { get; set; }
	[JsonPropertyName("family_name")]
	public string? FamilyName { get; set; }

	[JsonPropertyName("roles")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<RoleResp>? Roles { get; set; }

	[JsonPropertyName("claims")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<ClaimResp>? Claims { get; set; }
}

/*----------------------------------------------------------------------*/

public struct RoleResp
{
	public static RoleResp BuildFromRole(BatRole role)
	{
		return new RoleResp
		{
			Id = role.Id,
			Name = role.Name ?? string.Empty,
			Description = role.Description ?? string.Empty,
			Claims = role.Claims?.Select(c => new ClaimResp { ClaimType = c.ClaimType!, ClaimValue = c.ClaimValue! }),
		};
	}

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("claims")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<ClaimResp>? Claims { get; set; }
}

/*----------------------------------------------------------------------*/

public struct ChangePasswordResp
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Message { get; set; }

	public string Token { get; set; }
}

/*----------------------------------------------------------------------*/

public struct AppResp
{
	public static AppResp BuildFromApp(Application app)
	{
		return new AppResp
		{
			Id = app.Id,
			DisplayName = app.DisplayName,
			PublicKeyPEM = app.PublicKeyPEM,
			CreatedAt = app.CreatedAt,
			UpdatedAt = app.UpdatedAt
		};
	}

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("display_name")]
	public string DisplayName { get; set; }

	[JsonPropertyName("public_key_pem")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? PublicKeyPEM { get; set; }

	[JsonPropertyName("created_at")]
	public DateTimeOffset CreatedAt { get; set; }

	[JsonPropertyName("updated_at")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public DateTimeOffset? UpdatedAt { get; set; }
}
