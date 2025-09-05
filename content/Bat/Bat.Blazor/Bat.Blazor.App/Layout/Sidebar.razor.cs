using Microsoft.AspNetCore.Components;

namespace Bat.Blazor.App.Layout;

public partial class Sidebar
{
	public struct SidebarItem
	{
		public string Id { get; set; }
		public string Icon { get; set; }
		public string Label { get; set; }
		public string Url { get; set; }
	}

	public struct SidebarSection
	{
		public string Id { get; set; }
		public string Label { get; set; }
		public IEnumerable<SidebarItem> Items { get; set; }
	}

	[Inject]
	protected NavigationManager NavigationManager { get; init; } = default!;

	private static readonly IDictionary<string, SidebarSection> _sidebarSections = new SortedDictionary<string, SidebarSection>(StringComparer.OrdinalIgnoreCase);

	public static void AddOrReplaceSection(SidebarSection section)
	{
		_sidebarSections[section.Id] = section;
	}

	private static SidebarSection[] Sections
	{
		get
		{
			return [.. _sidebarSections.Values];
		}
	}
}
