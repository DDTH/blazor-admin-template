using System.Text.Json.Serialization;

namespace Bat.Shared.Api;

/// <summary>
/// Request info for the <see cref="IApiClient.GetExternalAuthUrlAsync(ExternalAuthUrlReq, string?, HttpClient?, CancellationToken)"/> method.
/// </summary>
public struct ExternalAuthUrlReq
{
	/// <summary>
	/// The external authentication provider.
	/// </summary>
	[JsonPropertyName("provider")]
	public string Provider { get; set; }

	/// <summary>
	/// The URL to redirect to after authentication.
	/// </summary>
	[JsonPropertyName("redirect_url")]
	public string RedirectUrl { get; set; }
}

/*----------------------------------------------------------------------*/

/// <summary>
/// Request info for the <see cref="IApiClient.ExternalLoginAsync(ExternalAuthReq, string?, HttpClient?, CancellationToken)"/> method.
/// </summary>
public struct ExternalAuthReq
{
	/// <summary>
	/// The external authentication provider.
	/// </summary>
	[JsonPropertyName("provider")]
	public string Provider { get; set; }

	/// <summary>
	/// The external authentication code.
	/// </summary>
	[JsonPropertyName("auth_data")]
	public IDictionary<string, string> AuthData { get; set; }
}

/// <summary>
/// Response info for the <see cref="IApiClient.ExternalLoginAsync(ExternalAuthReq, string?, HttpClient?, CancellationToken)"/> API call.
/// </summary>
public struct ExternalAuthResp
{
	/// <summary>
	/// Convenience method to create a new ExternalAuthResp instance.
	/// </summary>
	/// <param name="provider"></param>
	/// <param name="status"></param>
	/// <param name="error"></param>
	/// <returns></returns>
	public static ExternalAuthResp New(string provider, int status, string error)
	{
		return new ExternalAuthResp { Provider = provider, Status = status, Error = error ?? "" };
	}

	/// <summary>
	/// Convenience method to create a new ExternalAuthResp instance.
	/// </summary>
	/// <param name="provider"></param>
	/// <param name="status"></param>
	/// <param name="token"></param>
	/// <param name="expiry"></param>
	/// <returns></returns>
	public static ExternalAuthResp New(string provider, int status, string token, DateTimeOffset? expiry)
	{
		return new ExternalAuthResp { Provider = provider, Status = status, Token = token ?? "", Expiry = expiry };
	}

	/// <summary>
	/// The external authentication provider.
	/// </summary>
	[JsonPropertyName("provider")]
	public string Provider { get; set; }

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
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Token { get; set; }

	/// <summary>
	/// When the token expires.
	/// </summary>
	[JsonPropertyName("expiry")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public DateTimeOffset? Expiry { get; set; }

	/// <summary>
	/// The URL to redirect to after authentication.
	/// </summary>
	[JsonPropertyName("return_url")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? ReturnUrl { get; set; }
}
