using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Bat.Shared.ExternalLoginHelper;

/// <summary>
/// Configurations for an external login provider.
/// </summary>
public sealed class ExternalLoginProviderConfig : Dictionary<string, string>
{
	public ExternalLoginProviderConfig() { }

	public ExternalLoginProviderConfig(string providerName)
	{
		ProviderName = providerName;
	}

	public ExternalLoginProviderConfig(string providerName, IDictionary<string, string> config) : base(config)
	{
		ProviderName = providerName;
	}

	public ExternalLoginProviderConfig(string providerName, IConfigurationSection config)
	{
		ProviderName = providerName;
		foreach (var conf in config.GetChildren().Where(c => c.Value != null))
		{
			this[conf.Key] = conf.Value!;
		}
	}

	public string ProviderName { get; internal set; } = default!;

	public bool TryGetValueAsBool(string key, out bool value)
	{
		if (TryGetValue(key, out var val) && bool.TryParse(val, out value))
		{
			return true;
		}
		value = false;
		return false;
	}

	public bool GetValueAsBool(string key)
	{
		return GetValueAsBool(key, false);
	}

	public bool GetValueAsBool(string key, bool defaultValue)
	{
		if (TryGetValueAsBool(key, out var value))
		{
			return value;
		}
		return defaultValue;
	}
}

public class BaseExternalProviderResp
{
	[JsonPropertyName("provider")]
	public string Provider { get; set; } = default!;

	[JsonPropertyName("status_code")]
	public int StatusCode { get; set; } = default!;

	[JsonIgnore]
	public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode < 300;

	[JsonPropertyName("error_type")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? ErrorType { get; set; }

	[JsonPropertyName("error_message")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? ErrorMessage { get; set; }
}

public sealed class ExternalLoginResult : BaseExternalProviderResp
{
	[JsonPropertyName("scope")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Scope { get; set; }

	[JsonPropertyName("token_type")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? TokenType { get; set; }

	[JsonPropertyName("access_token")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? AccessToken { get; set; }

	[JsonPropertyName("refresh_token")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? RefreshToken { get; set; }

	[JsonPropertyName("expire_in")]
	public int ExpireIn { get; set; }

	[JsonPropertyName("expire_at")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public DateTimeOffset? ExpireAt { get; set; }

	[JsonPropertyName("redirect_url")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? RedirectUrl { get; set; }
}

public sealed class ExternalUserProfile : BaseExternalProviderResp
{
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Id { get; set; }

	[JsonPropertyName("email")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Email { get; set; }

	[JsonPropertyName("given_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? GivenName { get; set; }

	[JsonPropertyName("family_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? FamilyName { get; set; }

	[JsonPropertyName("display_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? DisplayName { get; set; }
}
