using Blazored.LocalStorage;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Bat.Blazor.App.Helpers;

public class LocalStorageHelper(ILocalStorageService localStorageService)
{
	private static readonly ConcurrentDictionary<string, string> Entries = new();

	private static bool IsBrowser => OperatingSystem.IsBrowser();

	public async ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
	{
		if (!IsBrowser)
		{
			var value = Entries.GetValueOrDefault(key);
			return !string.IsNullOrEmpty(value) ? JsonSerializer.Deserialize<T>(value) : default;
		}
		return await localStorageService.GetItemAsync<T>(key, cancellationToken);
	}

	public async ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default)
	{
		var jdata = JsonSerializer.Serialize(data);
		Entries.AddOrUpdate(key, jdata, (_, _) => jdata);
		try
		{
			await localStorageService.SetItemAsync(key, data, cancellationToken);
		}
		catch (InvalidOperationException)
		{
		}
	}

	public async ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default)
	{
		Entries.TryRemove(key, out _);
		try
		{
			await localStorageService.RemoveItemAsync(key, cancellationToken);
		}
		catch (InvalidOperationException)
		{
		}
	}

	public async ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
	{
		foreach (var key in keys)
		{
			Entries.TryRemove(key, out _);
		}
		try
		{
			await localStorageService.RemoveItemsAsync(keys, cancellationToken);
		}
		catch (InvalidOperationException)
		{
		}
	}
}
