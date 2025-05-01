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
		public string Icon { get; set; }
		public string Label { get; set; }
	}

	private static readonly IDictionary<string, SidebarSection> _sidebarSections = new SortedDictionary<string, SidebarSection>(StringComparer.OrdinalIgnoreCase);
	private static readonly IDictionary<string, List<SidebarItem>> _sidebarItems = new Dictionary<string, List<SidebarItem>>();

	public static void AddOrReplaceSection(SidebarSection section)
	{
		_sidebarSections[section.Id] = section;
	}

	public static bool AddItem(string sectionId, SidebarItem item)
	{
		if (!_sidebarSections.ContainsKey(sectionId))
		{
			return false;
		}
		var sectionItems = _sidebarItems.TryGetValue(sectionId, out var items) ? items : [];
		_sidebarItems[sectionId] = sectionItems;
		sectionItems.Add(item);
		return true;
	}

	private List<SidebarSection> Sections
	{
		get
		{
			return [.. _sidebarSections.Values];
		}
	}
}
