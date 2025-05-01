using System.Text.Json.Serialization;
using Bat.Demo.Shared.Models;

namespace Bat.Demo.Api.Api;

/// <summary>
/// Request to create or update an application.
/// </summary>
public struct CreateOrUpdateAppReq
{
	[JsonPropertyName("display_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? DisplayName { get; set; }

	[JsonPropertyName("public_key_pem")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? PublicKeyPEM { get; set; }
}

/// <summary>
/// Response structure for APIs that return application information.
/// </summary>
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
