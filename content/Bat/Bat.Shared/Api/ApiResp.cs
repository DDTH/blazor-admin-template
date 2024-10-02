﻿using System.Text.Json.Serialization;

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
/// Response to the <c>/info</c> API call.
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