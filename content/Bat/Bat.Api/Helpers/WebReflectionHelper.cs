using System.Reflection;

namespace Bat.Api.Helpers;

public class WebReflectionHelper : Shared.Helpers.ReflectionHelper
{
	public static async Task InvokeAsyncMethod(WebApplicationBuilder appBuilder, Type typeInfo, MethodInfo methodInfo)
	{
		// TODO: ASP0000 - calling IServiceCollection.BuildServiceProvider results in more than one copy of singleton
		// services being created which might result in incorrect application behavior.
		// Proposed workaround/fix: special treat for IOptions<T>, ILoggerFactory and ILogger<T>?
		var serviceProvider = appBuilder.Services.BuildServiceProvider();
		await InvokeAsyncMethod(serviceProvider, [appBuilder, appBuilder.Services], typeInfo, methodInfo);
	}

	public static void InvokeMethod(WebApplicationBuilder appBuilder, Type typeInfo, MethodInfo methodInfo)
	{
		// TODO: ASP0000 - calling IServiceCollection.BuildServiceProvider results in more than one copy of singleton
		// services being created which might result in incorrect application behavior.
		// Proposed workaround/fix: special treat for IOptions<T>, ILoggerFactory and ILogger<T>?
		var serviceProvider = appBuilder.Services.BuildServiceProvider();
		InvokeMethod(serviceProvider, [appBuilder, appBuilder.Services], typeInfo, methodInfo);
	}

	public static async Task InvokeAsyncMethod(WebApplication app, Type typeInfo, MethodInfo methodInfo)
	{
		await InvokeAsyncMethod(app.Services, [app, app.Services], typeInfo, methodInfo);
	}

	public static void InvokeMethod(WebApplication app, Type typeInfo, MethodInfo methodInfo)
	{
		InvokeMethod(app.Services, [app, app.Services], typeInfo, methodInfo);
	}
}
