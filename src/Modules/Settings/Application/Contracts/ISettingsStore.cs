namespace RMS.Modules.Settings.Application.Contracts;

/// <summary>
/// Read side for settings. Returns the raw key/value map so the application layer
/// can project it into strongly typed view models.
/// </summary>
public interface ISettingsReadStore
{
    Task<IReadOnlyDictionary<string, string?>> GetAllValuesAsync(CancellationToken cancellationToken = default);
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Write side for settings. Settings are keyed by a stable key and upserted.
/// </summary>
public interface ISettingsWriteStore
{
    Task UpsertAsync(string key, string? value, CancellationToken cancellationToken = default);
    Task UpsertManyAsync(IEnumerable<KeyValuePair<string, string?>> values, CancellationToken cancellationToken = default);
    Task ResetToDefaultsAsync(CancellationToken cancellationToken = default);
}
