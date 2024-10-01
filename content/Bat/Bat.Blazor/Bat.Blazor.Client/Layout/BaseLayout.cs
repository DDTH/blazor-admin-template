using Bat.Shared.Api;
using Microsoft.AspNetCore.Components;

namespace Bat.Blazor.Client.Layout;

/// <summary>
/// Base layout class that provides common properties and utility methods.
/// </summary>
public abstract class BaseLayout : LayoutComponentBase
{
	/// <summary>
	/// Check if the component is rendered in WASM mode.
	/// </summary>
	protected virtual bool IsBrowser { get => OperatingSystem.IsBrowser(); }

	[Inject]
	protected virtual IServiceProvider ServiceProvider { get; init; } = default!;

	protected virtual IApiClient ApiClient
	{
		get
		{
			return ServiceProvider.GetRequiredService<IApiClient>();
		}
	}
}
