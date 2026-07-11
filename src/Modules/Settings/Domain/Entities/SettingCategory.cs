namespace RMS.Modules.Settings.Domain.Entities;

/// <summary>
/// Top-level grouping used to organize settings in the administration UI
/// (e.g. General, Receipts, Sales, File Storage). Ordered by <see cref="SortOrder"/>.
/// </summary>
public sealed record SettingCategory(
    int Id,
    string Name,
    string DisplayName,
    int SortOrder)
{
    public SettingCategory() : this(0, string.Empty, string.Empty, 0) { }
}
