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
