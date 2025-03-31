using System.Text.Json;

namespace Bat.Shared.Helpers;

/// <summary>
/// The response from an HTTP request, with the content parsed as JSON.
/// </summary>
public struct HttpResponseJson
{
	public int StatusCode { get; set; }

	public readonly bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode < 300;

	public Exception? Error { get; internal set; }

	public HttpResponseJson() { }

	public HttpResponseJson(int statusCode)
	{
		StatusCode = statusCode;
	}

	public HttpResponseJson(int statusCode, string content)
	{
		StatusCode = statusCode;
		Content = content;
	}

	private string _content = string.Empty;
	public string Content
	{
		readonly get => _content;
		set
		{
			_content = value;
			try
			{
				ContentJson = JsonDocument.Parse(_content);
				Error = null;
			}
			catch (JsonException ex)
			{
				ContentJson = null;
				Error = ex;
			}
		}
	}

	public JsonDocument? ContentJson { get; internal set; }
}

public static class HttpClientHelper
{
	/// <summary>
	/// Sends an HTTP request and returns the response as JSON.
	/// </summary>
	/// <param name="httpClient"></param>
	/// <param name="req"></param>
	/// <returns></returns>
	public static async Task<HttpResponseJson> HttpRequestThatReturnsJson(HttpClient httpClient, HttpRequestMessage req)
	{
		try
		{
			using var resp = await httpClient.SendAsync(req);
			var content = await resp.Content.ReadAsStringAsync();
			return new HttpResponseJson
			{
				StatusCode = (int)resp.StatusCode,
				Content = content,
			};
		}
		catch (Exception ex) when (ex is ArgumentNullException or InvalidOperationException or HttpRequestException or TaskCanceledException)
		{
			return new HttpResponseJson
			{
				StatusCode = 0,
				Error = ex,
			};
		}
	}
}
