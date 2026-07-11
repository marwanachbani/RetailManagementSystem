namespace RMS.Modules.Settings.Domain.Entities;

/// <summary>
/// A single configuration value. Settings are stored as a flat key/value table
/// grouped by category so administrators can manage them entirely from the UI
/// without editing configuration files.
/// </summary>
public sealed record Setting(
    string Key,
    string Category,
    string? Value,
    SettingDataType DataType,
    string? Description)
{
    public Setting() : this(string.Empty, string.Empty, null, SettingDataType.String, null) { }
}
