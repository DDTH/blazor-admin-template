using Bat.Blazor.Client.Helpers;
using Bat.Shared.Bootstrap;
using Bat.Shared.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;

namespace Bat.Blazor.Client;

/// <summary>
/// Utility class to bootstrap Blazor Client applications.
/// </summary>
public sealed class WasmAppBootstrapper
{
	private static readonly string[] methodNameConfigureBuilder = { "ConfigureWasmBuilder", "ConfiguresWasmBuilder" };
	private static readonly string[] methodNameConfigureBuilderAsync = { "ConfigureWasmBuilderAsync", "ConfiguresWasmBuilderAsync" };
	private static readonly string[] methodNameDecorateApp = { "DecorateWasmApp", "DecoratesWasmApp", "DecorateWasmApplication", "DecoratesWasmApplication" };
	private static readonly string[] methodNameDecorateAppAsync = { "DecorateWasmAppAsync", "DecoratesWasmAppAsync", "DecorateWasmApplicationAsync", "DecoratesWasmApplicationAsync" };

	public static ICollection<Task> Bootstrap(WebAssemblyHostBuilder wasmAppBuilder, out WebAssemblyHost app)
	{
		var bootstrappersInfo = new List<BootstrapperStruct>();
		AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
			.Where(t => t.IsClass && !t.IsAbstract && t.IsDefined(typeof(BootstrapperAttribute), false))
			.ToList()
			.ForEach(t =>
			{
				Console.WriteLine($"[INFO] Found bootstrapper: {t.FullName}.");

				var methodConfigureBuilder = t.GetMethods().FirstOrDefault(m => m.IsPublic && methodNameConfigureBuilder.Contains(m.Name));
				var methodConfigureBuilderAsync = t.GetMethods().FirstOrDefault(m => m.IsPublic && methodNameConfigureBuilderAsync.Contains(m.Name));
				var methodDecorateApp = t.GetMethods().FirstOrDefault(m => m.IsPublic && methodNameDecorateApp.Contains(m.Name));
				var methodDecorateAppAsync = t.GetMethods().FirstOrDefault(m => m.IsPublic && methodNameDecorateAppAsync.Contains(m.Name));
				if (methodConfigureBuilder == null && methodDecorateApp == null && methodConfigureBuilderAsync == null && methodDecorateAppAsync == null)
				{
					var allMethods = string.Join(", ", methodNameConfigureBuilder.Concat(methodNameConfigureBuilderAsync).Concat(methodNameDecorateApp).Concat(methodNameDecorateAppAsync));
					Console.WriteLine($"[WARN] {t.FullName}...couldnot find any public method: {allMethods}.");
					return;
				}
				if (methodConfigureBuilderAsync != null && !AsyncHelper.IsAsyncMethod(methodConfigureBuilderAsync))
				{
					Console.WriteLine($"[WARN] {t.FullName}...found method {methodConfigureBuilderAsync.Name} but it is not async.");
					return;
				}
				if (methodDecorateAppAsync != null && !AsyncHelper.IsAsyncMethod(methodDecorateAppAsync))
				{
					Console.WriteLine($"[WARN] {t.FullName}...found method {methodDecorateAppAsync.Name} but it is not async.");
					return;
				}
				var attr = t.GetCustomAttribute<BootstrapperAttribute>();
				var priority = attr?.Priority ?? 1000;
				var bootstrapper = new BootstrapperStruct(t, methodConfigureBuilder, methodConfigureBuilderAsync, methodDecorateApp, methodDecorateAppAsync, priority);
				bootstrappersInfo.Add(bootstrapper);

				var foundMethods = new List<string>();
				if (methodConfigureBuilderAsync != null) foundMethods.Add(methodConfigureBuilderAsync.Name);
				if (methodConfigureBuilder != null) foundMethods.Add(methodConfigureBuilder.Name);
				if (methodDecorateAppAsync != null) foundMethods.Add(methodDecorateAppAsync.Name);
				if (methodDecorateApp != null) foundMethods.Add(methodDecorateApp.Name);
				Console.WriteLine($"[INFO] {t.FullName}...found methods: {string.Join(", ", foundMethods)}.");
			});

		bootstrappersInfo.Sort((a, b) => a.priority.CompareTo(b.priority));

		var backgroundBootstrappingTasks = Array.Empty<Task>();
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
