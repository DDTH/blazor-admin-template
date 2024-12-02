using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Bat.Libs.Opurator;

/// <summary>
/// Service that help executing tasks in a controlled manner.
/// </summary>
public interface ITaskOperator
{
	/// <summary>
	/// Executes the action only once throughout the application lifetime.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="action"></param>
	/// <returns><c>true</c> if the task is scheduled to execute.</returns>
	public Task<bool> ExecuteOnlyOnceAsync(string id, Action action);

	/// <summary>
	/// Executes the action if the action with the same ID is not executing.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="action"></param>
	/// <returns><c>true</c> if the task is scheduled to execute.</returns>
	public Task<bool> ExecuteNonParallelAsync(string id, Action action);
}

/// <summary>
/// Built-in implementation of <see cref="ITaskOperator"/>.
/// </summary>
public class TaskOperator : ITaskOperator
{
	private static readonly ConcurrentDictionary<string, bool> _executingIds = new();
	private static readonly ConcurrentDictionary<string, bool> _executedIds = new();

	/// <inheritdoc/>
	public async Task<bool> ExecuteOnlyOnceAsync(string id, Action action)
	{
		if (_executedIds.TryAdd(id, true))
		{
			await Task.Run(action);
			return true;
		}
		return false;
	}

	/// <inheritdoc/>
	public async Task<bool> ExecuteNonParallelAsync(string id, Action action)
	{
		if (_executingIds.TryAdd(id, true))
		{
			await Task.Run(() =>
			{
				try { action(); }
				finally { _executingIds.TryRemove(id, out _); }
			});
			return true;
		}
		return false;
	}
}

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register <see cref="ITaskOperator"/>.
/// </summary>
public static class IServiceCollectionTaskOperatorExtensions
{
	public static IServiceCollection AddTaskOperator(this IServiceCollection services)
	{
		services.AddSingleton<ITaskOperator, TaskOperator>();
		return services;
	}
}
