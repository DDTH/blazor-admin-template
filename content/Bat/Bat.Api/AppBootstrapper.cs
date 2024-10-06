using Bat.Api.Helpers;
using Bat.Shared.Bootstrap;
using System.Reflection;

namespace Bat.Api;

/// <summary>
/// Utility class to bootstrap WebAPI and Blazor Server applications.
/// </summary>
public sealed class AppBootstrapper
{
	private static readonly ILogger<AppBootstrapper> logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<AppBootstrapper>();

	private static readonly string[] methodNamesConfigureServices = { "ConfigureServices", "ConfiguresServices", "ConfigureService", "ConfiguresService" };
	private static readonly string[] methodNamesConfigureServicesAsync = { "ConfigureServicesAsync", "ConfiguresServicesAsync", "ConfigureServiceAsync", "ConfiguresServiceAsync" };
	private static readonly string[] methodNamesConfigureBuilder = { "ConfigureBuilder", "ConfiguresBuilder" };
	private static readonly string[] methodNamesConfigureBuilderAsync = { "ConfigureBuilderAsync", "ConfiguresBuilderAsync" };
	private static readonly string[] methodNamesDecorateApp = { "DecorateApp", "DecoratesApp", "DecorateApplication", "DecoratesApplication" };
	private static readonly string[] methodNamesDecorateAppAsync = { "DecorateAppAsync", "DecoratesAppAsync", "DecorateApplicationAsync", "DecoratesApplicationAsync" };

	public static ICollection<Task> Bootstrap(out WebApplication app, WebApplicationBuilder appBuilder, IEnumerable<Assembly>? assemblies = default)
	{
		assemblies = assemblies?.Distinct() ?? AppDomain.CurrentDomain.GetAssemblies();

		var bootstrappersInfo = new List<BootstrapperStruct>();
		logger.LogInformation("Loading bootstrappers...");
		BootstrapHelper.FindBootstrappers(assemblies).ToList().ForEach(t =>
		{
			logger.LogInformation("Found bootstrapper: {name}.", t.FullName);
			var lookupMethodConfigureServicesAsync = new MethodLookup { MethodNamesToFind = methodNamesConfigureServicesAsync };
			var lookupMethodConfigureServices = new MethodLookup { MethodNamesToFind = methodNamesConfigureServices };
			var lookupMethodConfigureBuilderAsync = new MethodLookup { MethodNamesToFind = methodNamesConfigureBuilderAsync };
			var lookupMethodConfigureBuilder = new MethodLookup { MethodNamesToFind = methodNamesConfigureBuilder };
			var lookupMethodDecorateAppAsync = new MethodLookup { MethodNamesToFind = methodNamesDecorateAppAsync };
			var lookupMethodDecorateApp = new MethodLookup { MethodNamesToFind = methodNamesDecorateApp };
			BootstrapHelper.FindMethods(t, [ lookupMethodConfigureServicesAsync, lookupMethodConfigureServices,
				lookupMethodConfigureBuilderAsync, lookupMethodConfigureBuilder,
				lookupMethodDecorateAppAsync, lookupMethodDecorateApp ]);
			if (lookupMethodConfigureServicesAsync.MethodInfo == null && lookupMethodConfigureServices.MethodInfo == null
				&& lookupMethodConfigureBuilderAsync.MethodInfo == null && lookupMethodConfigureBuilder.MethodInfo == null
				&& lookupMethodDecorateAppAsync.MethodInfo == null && lookupMethodDecorateApp.MethodInfo == null)
			{
				logger.LogWarning("{name}...couldnot find any public method: {ConfigureServicesAsync}, {ConfigureServices}, {ConfigureBuilderAsync}, {ConfigureBuilder}, {DecorateAppAsync}, {DecorateApp}.",
					t.FullName,
					methodNamesConfigureServicesAsync, methodNamesConfigureServices,
					methodNamesConfigureBuilderAsync, methodNamesConfigureBuilder,
					methodNamesDecorateAppAsync, methodNamesDecorateApp);
				return;
			}
			var asyncMethods = new MethodInfo?[]
			{
				lookupMethodConfigureServicesAsync.MethodInfo,
				lookupMethodConfigureBuilderAsync.MethodInfo,
				lookupMethodDecorateAppAsync.MethodInfo
			};
			var invalidAsyncMethod = BootstrapHelper.VerifyAsyncMethods(asyncMethods);
			if (invalidAsyncMethod != null)
			{
				logger.LogWarning("{name}...found method {method} but it is not async.", t.FullName, invalidAsyncMethod.Name);
				return;
			}
			var attr = t.GetCustomAttribute<BootstrapperAttribute>();
			var priority = attr?.Priority ?? 1000;
			var bootstrapper = new BootstrapperStruct(t, priority: priority,
				methodConfigureServices: lookupMethodConfigureServices.MethodInfo, methodConfigureServicesAsync: lookupMethodConfigureServicesAsync.MethodInfo,
				methodConfigureBuilder: lookupMethodConfigureBuilder.MethodInfo, methodConfigureBuilderAsync: lookupMethodConfigureBuilderAsync.MethodInfo,
				methodDecorateApp: lookupMethodDecorateApp.MethodInfo, methodDecorateAppAsync: lookupMethodDecorateAppAsync.MethodInfo);
			bootstrappersInfo.Add(bootstrapper);

			var foundMethods = new List<string>();
			if (lookupMethodConfigureServicesAsync.MethodInfo != null) foundMethods.Add(lookupMethodConfigureServicesAsync.MethodInfo.Name);
			if (lookupMethodConfigureServices.MethodInfo != null) foundMethods.Add(lookupMethodConfigureServices.MethodInfo.Name);
			if (lookupMethodConfigureBuilderAsync.MethodInfo != null) foundMethods.Add(lookupMethodConfigureBuilderAsync.MethodInfo.Name);
			if (lookupMethodConfigureBuilder.MethodInfo != null) foundMethods.Add(lookupMethodConfigureBuilder.MethodInfo.Name);
			if (lookupMethodDecorateAppAsync.MethodInfo != null) foundMethods.Add(lookupMethodDecorateAppAsync.MethodInfo.Name);
			if (lookupMethodDecorateApp.MethodInfo != null) foundMethods.Add(lookupMethodDecorateApp.MethodInfo.Name);
			logger.LogInformation("{name}...found methods: {methods}.", t.FullName, string.Join(", ", foundMethods));
		});

		bootstrappersInfo.Sort((a, b) => a.priority.CompareTo(b.priority));
		var backgroundBootstrappingTasks = Array.Empty<Task>();

		logger.LogInformation("========== [Bootstrapping] Configuring services...");
		foreach (var bootstrapper in bootstrappersInfo)
		{
			if (bootstrapper.methodConfigureServicesAsync == null && bootstrapper.methodConfigureServices == null)
			{
				continue;
			}

			if (bootstrapper.methodConfigureServicesAsync != null)
			{
				logger.LogInformation("[{priority}] Invoking async method {type}.{method}...",
					bootstrapper.priority, bootstrapper.type.FullName, bootstrapper.methodConfigureServicesAsync.Name);

				// async method takes priority
				var task = WebReflectionHelper.InvokeAsyncMethod(appBuilder, bootstrapper.type, bootstrapper.methodConfigureServicesAsync);
				backgroundBootstrappingTasks.Append(task);
			}
			else
			{
				logger.LogInformation("[{priority}] Invoking method {type}.{method}...",
					bootstrapper.priority, bootstrapper.type.FullName, bootstrapper.methodConfigureServices!.Name);
				WebReflectionHelper.InvokeMethod(appBuilder, bootstrapper.type, bootstrapper.methodConfigureServices);
			}
		}

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
				var task = WebReflectionHelper.InvokeAsyncMethod(appBuilder, bootstrapper.type, bootstrapper.methodConfigureBuilderAsync);
				backgroundBootstrappingTasks.Append(task);
			}
			else
			{
				logger.LogInformation("[{priority}] Invoking method {type}.{method}...",
					bootstrapper.priority, bootstrapper.type.FullName, bootstrapper.methodConfigureBuilder!.Name);
				WebReflectionHelper.InvokeMethod(appBuilder, bootstrapper.type, bootstrapper.methodConfigureBuilder);
			}
		}

		app = appBuilder.Build();

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
				var task = WebReflectionHelper.InvokeAsyncMethod(app, bootstrapper.type, bootstrapper.methodDecorateAppAsync);
				backgroundBootstrappingTasks.Append(task);
			}
			else
			{
				logger.LogInformation("[{priority}] Invoking method {type}.{method}...",
					bootstrapper.priority, bootstrapper.type.FullName, bootstrapper.methodDecorateApp!.Name);
				WebReflectionHelper.InvokeMethod(app, bootstrapper.type, bootstrapper.methodDecorateApp);
			}
		}

		return backgroundBootstrappingTasks;
	}
}
