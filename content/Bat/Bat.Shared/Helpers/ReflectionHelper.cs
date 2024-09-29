using Ddth.Utilities;
using System.Reflection;

namespace Bat.Shared.Helpers;

public class ReflectionHelper
{
	protected static async Task InvokeAsyncMethod(IServiceProvider? serviceProvider, IEnumerable<object?>? services, Type typeInfo, MethodInfo methodInfo)
	{
		var paramsInfo = methodInfo.GetParameters();
		var parameters = ReflectionDIHelper.BuildDIParams(serviceProvider, services, paramsInfo);
		object? instance = null;
		if (!methodInfo.IsStatic)
		{
			instance = ReflectionDIHelper.CreateInstance<object>(serviceProvider, services, typeInfo);
		}
		await (dynamic)methodInfo.Invoke(instance, parameters)!;
	}

	protected static void InvokeMethod(IServiceProvider? serviceProvider, IEnumerable<object?>? services, Type typeInfo, MethodInfo methodInfo)
	{
		var paramsInfo = methodInfo.GetParameters();
		var parameters = ReflectionDIHelper.BuildDIParams(serviceProvider, services, paramsInfo);
		object? instance = null;
		if (!methodInfo.IsStatic)
		{
			instance = ReflectionDIHelper.CreateInstance<object>(serviceProvider, services, typeInfo);
		}
		methodInfo.Invoke(instance, parameters);
	}
}
