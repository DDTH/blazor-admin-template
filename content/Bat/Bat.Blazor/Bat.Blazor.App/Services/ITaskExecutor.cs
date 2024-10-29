using System.Collections.Concurrent;

namespace Bat.Blazor.App.Services;

/// <summary>
/// Service that help executing tasks.
/// </summary>
public interface ITaskExecutor
{
	/// <summary>
	/// Execute the action only once throughout the application lifetime.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="action"></param>
	public Task ExecuteOnlyOnceAsync(string id, Action action);

	/// <summary>
	/// Execute the action if the action with the same ID is not executing.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="action"></param>
	public Task ExecuteNonParallelAsync(string id, Action action);
}

public class TaskExecutor : ITaskExecutor
{
	private static readonly ConcurrentDictionary<string, bool> _executingIds = new();
	private static readonly ConcurrentDictionary<string, bool> _executedIds = new();

	/// <inheritdoc/>
	public async Task ExecuteOnlyOnceAsync(string id, Action action)
	{
		if (_executedIds.ContainsKey(id))
		{
			return;
		}
		if (_executedIds.TryAdd(id, true))
		{
			await Task.Run(action);
		}
	}

	/// <inheritdoc/>
	public async Task ExecuteNonParallelAsync(string id, Action action)
	{
		if (_executingIds.ContainsKey(id))
		{
			return;
		}
		if (_executingIds.TryAdd(id, true))
		{
			_executedIds.TryAdd(id, true);
			await Task.Run(() =>
			{
				action();
				_executingIds.TryRemove(id, out _);
			});
		}
	}
}
