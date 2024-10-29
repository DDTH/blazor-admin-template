using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Bat.Shared.Helpers;
public sealed class AsyncHelper
{
	/// <summary>
	/// Waits for all background tasks to complete.
	/// </summary>
	/// <param name="tasks"></param>
	/// <param name="logger"></param>
	public static async void WaitForBackgroundTasks(ICollection<Task> tasks, ILogger? logger = default)
	{
		while (tasks.Count > 0)
		{
			var finishedTask = await Task.WhenAny(tasks);
			try { await finishedTask; }
			catch (Exception e)
			{
				logger?.LogError(e, "Error executing bootstrapper task.");
			}
			tasks.Remove(finishedTask);
		}
	}

	public static bool IsAsyncMethod(MethodInfo method) => method.ReturnType == typeof(Task)
		|| method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
}
