using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;

namespace Bat.Blazor.Client.Helpers;

public class BlazorClientReflectionHelper : Shared.Helpers.ReflectionHelper
{
	public static async Task InvokeAsyncMethod(WebAssemblyHostBuilder wasmAppBuilder, Type typeInfo, MethodInfo methodInfo)
	{
		// TODO: ASP0000 - calling IServiceCollection.BuildServiceProvider results in more than one copy of singleton
		// services being created which might result in incorrect application behavior.
		// Proposed workaround/fix: special treat for IOptions<T>, ILoggerFactory and ILogger<T>?
		var serviceProvider = wasmAppBuilder.Services.BuildServiceProvider();
		await InvokeAsyncMethod(serviceProvider, [wasmAppBuilder, wasmAppBuilder.Services], typeInfo, methodInfo);
	}

	public static void InvokeMethod(WebAssemblyHostBuilder wasmAppBuilder, Type typeInfo, MethodInfo methodInfo)
	{
		// TODO: ASP0000 - calling IServiceCollection.BuildServiceProvider results in more than one copy of singleton
		// services being created which might result in incorrect application behavior.
		// Proposed workaround/fix: special treat for IOptions<T>, ILoggerFactory and ILogger<T>?
		var serviceProvider = wasmAppBuilder.Services.BuildServiceProvider();
		InvokeMethod(serviceProvider, [wasmAppBuilder, wasmAppBuilder.Services], typeInfo, methodInfo);
	}

	public static async Task InvokeAsyncMethod(WebAssemblyHost wasmApp, Type typeInfo, MethodInfo methodInfo)
	{
		await InvokeAsyncMethod(wasmApp.Services, [wasmApp, wasmApp.Services], typeInfo, methodInfo);
	}

	public static void InvokeMethod(WebAssemblyHost wasmApp, Type typeInfo, MethodInfo methodInfo)
	{
		InvokeMethod(wasmApp.Services, [wasmApp, wasmApp.Services], typeInfo, methodInfo);
	}
}
