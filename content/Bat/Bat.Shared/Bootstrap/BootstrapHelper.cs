using Bat.Shared.Helpers;
using System.Reflection;

namespace Bat.Shared.Bootstrap;
public static class BootstrapHelper
{
	public static IEnumerable<Type> FindBootstrappers(IEnumerable<Assembly> assemblies)
	{
		return assemblies.SelectMany(a => a.GetTypes())
			.Where(t => t.IsClass && !t.IsAbstract && t.IsDefined(typeof(BootstrapperAttribute), false));
	}

	public static void FindMethods(Type typ, MethodLookup[] methodLookups)
	{
		for (var i = 0; i < methodLookups.Length; i++)
		{
			var ml = methodLookups[i];
			ml.MethodInfo = typ.GetMethods().FirstOrDefault(m => m.IsPublic && ml.MethodNamesToFind.Contains(m.Name));
		}
	}

	public static MethodInfo? VerifyAsyncMethods(IEnumerable<MethodInfo?> methods)
	{
		MethodInfo? invalidAsyncMethod = null;
		methods.Any(m =>
		{
			if (m != null && !AsyncHelper.IsAsyncMethod(m))
			{
				invalidAsyncMethod = m;
				return true;
			}
			return false;
		});
		return invalidAsyncMethod;
	}
}

public class MethodLookup
{
	public IEnumerable<string> MethodNamesToFind { get; set; } = default!;
	public MethodInfo? MethodInfo { get; set; }
}
