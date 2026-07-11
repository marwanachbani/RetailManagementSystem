namespace RMS.Modules.Settings.Domain;

/// <summary>
/// Logical data type of a persisted setting value. Used to validate conversions
/// and to present the appropriate editor in the UI.
/// </summary>
public enum SettingDataType
{
    String = 0,
    Integer = 1,
    Decimal = 2,
    Boolean = 3
}
