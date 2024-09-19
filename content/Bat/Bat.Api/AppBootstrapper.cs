﻿using Bat.Api.Bootstrap;
using Bat.Api.Helpers;
using System.Reflection;

namespace Bat.Api;

public class AppBootstrapper
{
	private static readonly ILogger<AppBootstrapper> logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<AppBootstrapper>();

	private static readonly string[] methodNameConfigureBuilder = { "ConfigureBuilder", "ConfiguresBuilder" };
	private static readonly string[] methodNameConfigureBuilderAsync = { "ConfigureBuilderAsync", "ConfiguresBuilderAsync" };
	private static readonly string[] methodNameDecorateApp = { "DecorateApp", "DecoratesApp", "DecorateApplication", "DecoratesApplication" };
	private static readonly string[] methodNameDecorateAppAsync = { "DecorateAppAsync", "DecoratesAppAsync", "DecorateApplicationAsync", "DecoratesApplicationAsync" };

	private static bool IsAsyncMethod(MethodInfo method) => method.ReturnType == typeof(Task)
		|| method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

	public static ICollection<Task> Bootstrap(WebApplicationBuilder appBuilder, out WebApplication app)
	{
		var bootstrappersInfo = new List<BootstrapperStruct>();

		logger.LogInformation("Loading bootstrappers...");
		AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
		.Where(t => t.IsClass && !t.IsAbstract && t.IsDefined(typeof(BootstrapperAttribute), false))
		.ToList()
		.ForEach(t =>
		{
			var methodConfigureBuilder = t.GetMethods().FirstOrDefault(m => m.IsPublic && methodNameConfigureBuilder.Contains(m.Name));
			var methodConfigureBuilderAsync = t.GetMethods().FirstOrDefault(m => m.IsPublic && methodNameConfigureBuilderAsync.Contains(m.Name));
			var methodDecorateApp = t.GetMethods().FirstOrDefault(m => m.IsPublic && methodNameDecorateApp.Contains(m.Name));
			var methodDecorateAppAsync = t.GetMethods().FirstOrDefault(m => m.IsPublic && methodNameDecorateAppAsync.Contains(m.Name));
			if (methodConfigureBuilder == null && methodDecorateApp == null && methodConfigureBuilderAsync == null && methodDecorateAppAsync == null)
			{
				logger.LogWarning("{name}...couldnot find any public method: {ConfigureBuilder}, {ConfigureBuilderAsync}, {DecorateApp}, {DecorateAppAsync}.",
					t.FullName, methodNameConfigureBuilder, methodNameConfigureBuilderAsync, methodNameDecorateApp, methodNameDecorateAppAsync);
				return;
			}
			if (methodConfigureBuilderAsync != null && !IsAsyncMethod(methodConfigureBuilderAsync))
			{
				logger.LogWarning("{name}...found method {ConfigureBuilderAsync} but it is not async.", t.FullName, methodConfigureBuilderAsync.Name);
				return;
			}
			if (methodDecorateAppAsync != null && !IsAsyncMethod(methodDecorateAppAsync))
			{
				logger.LogWarning("{name}...found method {DecorateAppAsync} but it is not async.", t.FullName, methodDecorateAppAsync.Name);
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
			logger.LogInformation("{name}...found methods: {methods}.", t.FullName, string.Join(", ", foundMethods));
		});

		bootstrappersInfo.Sort((a, b) => a.priority.CompareTo(b.priority));

		var backgroundBootstrappingTasks = Array.Empty<Task>();
		logger.LogInformation("========== [Bootstrapping] Configuring builder...");
		foreach (var bootstrapper in bootstrappersInfo)
		{
			if (bootstrapper.methodConfigureBuilderAsync == null && bootstrapper.methodConfigureBuilder == null)
			{
				continue;
			}

			if (bootstrapper.methodConfigureBuilderAsync != null)
			{
				logger.LogInformation("[{priority}] Invoking async method {type}.{method}...",
					bootstrapper.priority, bootstrapper.type.FullName, bootstrapper.methodConfigureBuilderAsync.Name);

				// async method takes priority
				var task = ReflectionHelper.InvokeAsyncMethod(appBuilder, bootstrapper.type, bootstrapper.methodConfigureBuilderAsync);
				backgroundBootstrappingTasks.Append(task);
			}
			else
			{
				logger.LogInformation("[{priority}] Invoking method {type}.{method}...",
					bootstrapper.priority, bootstrapper.type.FullName, bootstrapper.methodConfigureBuilder!.Name);
				ReflectionHelper.InvokeMethod(appBuilder, bootstrapper.type, bootstrapper.methodConfigureBuilder);
			}
		}

		app = appBuilder.Build();
		Global.App = app;
		logger.LogInformation("WebApplication instance has been created and added to Global.");

		logger.LogInformation("========== [Bootstrapping] Decorating application...");
		foreach (var bootstrapper in bootstrappersInfo)
		{
			if (bootstrapper.methodDecorateAppAsync == null && bootstrapper.methodDecorateApp == null)
			{
				continue;
			}

			if (bootstrapper.methodDecorateAppAsync != null)
			{
				logger.LogInformation("[{priority}] Invoking async method {type}.{method}...",
					bootstrapper.priority, bootstrapper.type.FullName, bootstrapper.methodDecorateAppAsync.Name);
				// async method takes priority
				var task = ReflectionHelper.InvokeAsyncMethod(app, bootstrapper.type, bootstrapper.methodDecorateAppAsync);
				backgroundBootstrappingTasks.Append(task);
			}
			else
			{
				logger.LogInformation("[{priority}] Invoking method {type}.{method}...",
					bootstrapper.priority, bootstrapper.type.FullName, bootstrapper.methodDecorateApp!.Name);
				ReflectionHelper.InvokeMethod(app, bootstrapper.type, bootstrapper.methodDecorateApp);
			}
		}

		return backgroundBootstrappingTasks;
	}

	public static async void WaitForBackgroundTasks(ICollection<Task> tasks)
	{
		while (tasks.Count > 0)
		{
			var finishedTask = await Task.WhenAny(tasks);
			try { await finishedTask; }
			catch (Exception e)
			{
				logger.LogError(e, "Error executing bootstrapper task.");
			}
			tasks.Remove(finishedTask);
		}
	}

	private readonly struct BootstrapperStruct(Type _type,
		MethodInfo? _methodConfigureBuilder, MethodInfo? _methodConfigureBuilderAsync,
		MethodInfo? _methodDecorateApp, MethodInfo? _methodDecorateAppAsync, int _priority = 1000)
	{
		public readonly Type type = _type;
		public readonly int priority = _priority;
		public readonly MethodInfo? methodConfigureBuilder = _methodConfigureBuilder;
		public readonly MethodInfo? methodConfigureBuilderAsync = _methodConfigureBuilderAsync;
		public readonly MethodInfo? methodDecorateApp = _methodDecorateApp;
		public readonly MethodInfo? methodDecorateAppAsync = _methodDecorateAppAsync;
	};
}
