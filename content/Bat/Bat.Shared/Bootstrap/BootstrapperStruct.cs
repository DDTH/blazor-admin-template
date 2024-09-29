using System.Reflection;

namespace Bat.Shared.Bootstrap;
public readonly struct BootstrapperStruct(Type _type,
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
