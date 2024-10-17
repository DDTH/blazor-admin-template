using System.Text.Json.Serialization;
using Bat.Shared.Models;

namespace Bat.Shared.Api;

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
