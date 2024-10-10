using Bat.Blazor.Client.Helpers;
using Bat.Shared.Bootstrap;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;

namespace Bat.Blazor.Client;

/// <summary>
/// Utility class to bootstrap Blazor Client applications.
/// </summary>
public sealed class WasmAppBootstrapper
{
	private static readonly string[] methodNamesConfigureServices = { "ConfigureServices", "ConfiguresServices", "ConfigureService", "ConfiguresService" };
	private static readonly string[] methodNamesConfigureServicesAsync = { "ConfigureServicesAsync", "ConfiguresServicesAsync", "ConfigureServiceAsync", "ConfiguresServiceAsync" };
	private static readonly string[] methodNamesConfigureBuilder = { "ConfigureWasmBuilder", "ConfiguresWasmBuilder" };
	private static readonly string[] methodNamesConfigureBuilderAsync = { "ConfigureWasmBuilderAsync", "ConfiguresWasmBuilderAsync" };
	private static readonly string[] methodNamesInitializeServices = { "InitializeServices", "InitializesServices", "InitializeService", "InitializesService" };
	private static readonly string[] methodNamesInitializeServicesAsync = { "InitializeServicesAsync", "InitializesServicesAsync", "InitializeServiceAsync", "InitializesServiceAsync" };
	private static readonly string[] methodNamesDecorateApp = { "DecorateWasmApp", "DecoratesWasmApp", "DecorateWasmApplication", "DecoratesWasmApplication" };
	private static readonly string[] methodNamesDecorateAppAsync = { "DecorateWasmAppAsync", "DecoratesWasmAppAsync", "DecorateWasmApplicationAsync", "DecoratesWasmApplicationAsync" };

	public static ICollection<Task> Bootstrap(out WebAssemblyHost app, WebAssemblyHostBuilder wasmAppBuilder, IEnumerable<Assembly>? assemblies = default)
	{
		assemblies = assemblies?.Distinct() ?? AppDomain.CurrentDomain.GetAssemblies();

		var bootstrappersInfo = new List<BootstrapperStruct>();
		Console.WriteLine("[INFO] Loading bootstrappers...");
		BootstrapHelper.FindBootstrappers(assemblies).ToList().ForEach(t =>
		{
			Console.WriteLine($"[INFO] Found bootstrapper: {t.FullName}.");
			var lookupMethodConfigureServicesAsync = new MethodLookup { MethodNamesToFind = methodNamesConfigureServicesAsync };
			var lookupMethodConfigureServices = new MethodLookup { MethodNamesToFind = methodNamesConfigureServices };
			var lookupMethodConfigureBuilderAsync = new MethodLookup { MethodNamesToFind = methodNamesConfigureBuilderAsync };
			var lookupMethodConfigureBuilder = new MethodLookup { MethodNamesToFind = methodNamesConfigureBuilder };
			var lookupMethodInitializeServicesAsync = new MethodLookup { MethodNamesToFind = methodNamesInitializeServicesAsync };
			var lookupMethodInitializeServices = new MethodLookup { MethodNamesToFind = methodNamesInitializeServices };
			var lookupMethodDecorateAppAsync = new MethodLookup { MethodNamesToFind = methodNamesDecorateAppAsync };
			var lookupMethodDecorateApp = new MethodLookup { MethodNamesToFind = methodNamesDecorateApp };
			BootstrapHelper.FindMethods(t, [ lookupMethodConfigureServicesAsync, lookupMethodConfigureServices,
				lookupMethodConfigureBuilderAsync, lookupMethodConfigureBuilder,
				lookupMethodInitializeServicesAsync,lookupMethodInitializeServices,
				lookupMethodDecorateAppAsync, lookupMethodDecorateApp ]);
			if (lookupMethodConfigureServicesAsync.MethodInfo == null && lookupMethodConfigureServices.MethodInfo == null
				&& lookupMethodConfigureBuilderAsync.MethodInfo == null && lookupMethodConfigureBuilder.MethodInfo == null
				&& lookupMethodInitializeServicesAsync.MethodInfo == null && lookupMethodInitializeServices.MethodInfo == null
				&& lookupMethodDecorateAppAsync.MethodInfo == null && lookupMethodDecorateApp.MethodInfo == null)
			{
				var allMethods = string.Join(", ", Array.Empty<string>().Concat(methodNamesConfigureServicesAsync).Concat(methodNamesConfigureServices)
					.Concat(methodNamesConfigureBuilderAsync).Concat(methodNamesConfigureBuilder)
					.Concat(methodNamesInitializeServicesAsync).Concat(methodNamesInitializeServices)
					.Concat(methodNamesDecorateAppAsync).Concat(methodNamesDecorateApp));
				Console.WriteLine($"[WARN] {t.FullName}...couldnot find any public method: {allMethods}.");
				return;
			}
			var asyncMethods = new MethodInfo?[]
			{
				lookupMethodConfigureServicesAsync.MethodInfo,
				lookupMethodConfigureBuilderAsync.MethodInfo,
				lookupMethodInitializeServicesAsync.MethodInfo,
				lookupMethodDecorateAppAsync.MethodInfo
			};
			var invalidAsyncMethod = BootstrapHelper.VerifyAsyncMethods(asyncMethods);
			if (invalidAsyncMethod != null)
			{
				Console.WriteLine($"[WARN] {t.FullName}...found method {invalidAsyncMethod.Name} but it is not async.");
				return;
			}
			var attr = t.GetCustomAttribute<BootstrapperAttribute>();
			var priority = attr?.Priority ?? 1000;
			var bootstrapper = new BootstrapperStruct(t, priority: priority,
				methodConfigureServices: lookupMethodConfigureServices.MethodInfo, methodConfigureServicesAsync: lookupMethodConfigureServicesAsync.MethodInfo,
				methodConfigureBuilder: lookupMethodConfigureBuilder.MethodInfo, methodConfigureBuilderAsync: lookupMethodConfigureBuilderAsync.MethodInfo,
				methodInitializeServices: lookupMethodInitializeServices.MethodInfo, methodInitializeServicesAsync: lookupMethodInitializeServicesAsync.MethodInfo,
				methodDecorateApp: lookupMethodDecorateApp.MethodInfo, methodDecorateAppAsync: lookupMethodDecorateAppAsync.MethodInfo);
			bootstrappersInfo.Add(bootstrapper);

			var foundMethods = new List<string>();
			if (lookupMethodConfigureServicesAsync.MethodInfo != null) foundMethods.Add(lookupMethodConfigureServicesAsync.MethodInfo.Name);
			if (lookupMethodConfigureServices.MethodInfo != null) foundMethods.Add(lookupMethodConfigureServices.MethodInfo.Name);
			if (lookupMethodConfigureBuilderAsync.MethodInfo != null) foundMethods.Add(lookupMethodConfigureBuilderAsync.MethodInfo.Name);
			if (lookupMethodConfigureBuilder.MethodInfo != null) foundMethods.Add(lookupMethodConfigureBuilder.MethodInfo.Name);
			if (lookupMethodInitializeServicesAsync.MethodInfo != null) foundMethods.Add(lookupMethodInitializeServicesAsync.MethodInfo.Name);
			if (lookupMethodInitializeServices.MethodInfo != null) foundMethods.Add(lookupMethodInitializeServices.MethodInfo.Name);
			if (lookupMethodDecorateAppAsync.MethodInfo != null) foundMethods.Add(lookupMethodDecorateAppAsync.MethodInfo.Name);
			if (lookupMethodDecorateApp.MethodInfo != null) foundMethods.Add(lookupMethodDecorateApp.MethodInfo.Name);
			Console.WriteLine($"[INFO] {t.FullName}...found methods: {string.Join(", ", foundMethods)}.");
		});

		bootstrappersInfo.Sort((a, b) => a.priority.CompareTo(b.priority));
		var backgroundBootstrappingTasks = Array.Empty<Task>();

		Console.WriteLine("[INFO] ========== [Bootstrapping] Configuring services...");
		foreach (var bootstrapper in bootstrappersInfo)
		{
			if (bootstrapper.methodConfigureServicesAsync == null && bootstrapper.methodConfigureServices == null)
			{
				continue;
			}

			if (bootstrapper.methodConfigureServicesAsync != null)
			{
				Console.WriteLine($"[{bootstrapper.priority}] Invoking async method {bootstrapper.type.FullName}.{bootstrapper.methodConfigureServicesAsync.Name}...");

				// async method takes priority
				var task = BlazorClientReflectionHelper.InvokeAsyncMethod(wasmAppBuilder, bootstrapper.type, bootstrapper.methodConfigureServicesAsync);
				backgroundBootstrappingTasks.Append(task);
			}
			else
			{
				Console.WriteLine($"[{bootstrapper.priority}] Invoking method {bootstrapper.type.FullName}.{bootstrapper.methodConfigureServices!.Name}...");
				BlazorClientReflectionHelper.InvokeMethod(wasmAppBuilder, bootstrapper.type, bootstrapper.methodConfigureServices);
			}
		}

		Console.WriteLine("[INFO] ========== [Bootstrapping] Configuring builder...");
		foreach (var bootstrapper in bootstrappersInfo)
		{
			if (bootstrapper.methodConfigureBuilderAsync == null && bootstrapper.methodConfigureBuilder == null)
			{
				continue;
			}

			if (bootstrapper.methodConfigureBuilderAsync != null)
			{
				Console.WriteLine($"[{bootstrapper.priority}] Invoking async method {bootstrapper.type.FullName}.{bootstrapper.methodConfigureBuilderAsync.Name}...");

				// async method takes priority
				var task = BlazorClientReflectionHelper.InvokeAsyncMethod(wasmAppBuilder, bootstrapper.type, bootstrapper.methodConfigureBuilderAsync);
				backgroundBootstrappingTasks.Append(task);
			}
			else
			{
				Console.WriteLine($"[{bootstrapper.priority}] Invoking method {bootstrapper.type.FullName}.{bootstrapper.methodConfigureBuilder!.Name}...");
				BlazorClientReflectionHelper.InvokeMethod(wasmAppBuilder, bootstrapper.type, bootstrapper.methodConfigureBuilder);
			}
		}

		app = wasmAppBuilder.Build();

		Console.WriteLine("[INFO] ========== [Bootstrapping] Initializing services...");
		foreach (var bootstrapper in bootstrappersInfo)
		{
			if (bootstrapper.methodInitializeServicesAsync == null && bootstrapper.methodInitializeServices == null)
			{
				continue;
			}

			if (bootstrapper.methodInitializeServicesAsync != null)
			{
				Console.WriteLine($"[{bootstrapper.priority}] Invoking async method {bootstrapper.type.FullName}.{bootstrapper.methodInitializeServicesAsync.Name}...");
				// async method takes priority
				var task = BlazorClientReflectionHelper.InvokeAsyncMethod(app, bootstrapper.type, bootstrapper.methodInitializeServicesAsync);
				backgroundBootstrappingTasks.Append(task);
			}
			else
			{
				Console.WriteLine($"[{bootstrapper.priority}] Invoking method {bootstrapper.type.FullName}.{bootstrapper.methodInitializeServices!.Name}...");
				BlazorClientReflectionHelper.InvokeMethod(app, bootstrapper.type, bootstrapper.methodInitializeServices);
			}
		}

		Console.WriteLine("[INFO] ========== [Bootstrapping] Decorating application...");
		foreach (var bootstrapper in bootstrappersInfo)
		{
			if (bootstrapper.methodDecorateAppAsync == null && bootstrapper.methodDecorateApp == null)
			{
				continue;
			}

			if (bootstrapper.methodDecorateAppAsync != null)
			{
				Console.WriteLine($"[{bootstrapper.priority}] Invoking async method {bootstrapper.type.FullName}.{bootstrapper.methodDecorateAppAsync.Name}...");
				// async method takes priority
				var task = BlazorClientReflectionHelper.InvokeAsyncMethod(app, bootstrapper.type, bootstrapper.methodDecorateAppAsync);
				backgroundBootstrappingTasks.Append(task);
			}
			else
			{
				Console.WriteLine($"[{bootstrapper.priority}] Invoking method {bootstrapper.type.FullName}.{bootstrapper.methodDecorateApp!.Name}...");
				BlazorClientReflectionHelper.InvokeMethod(app, bootstrapper.type, bootstrapper.methodDecorateApp);
			}
		}

		return backgroundBootstrappingTasks;
	}
}
