using System.IO;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Application.Models;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.Services;
using RMS.Modules.Settings.Domain;

namespace RMS.WPF.Backup;

/// <summary>
/// Resolves the effective backup configuration from the existing Settings module.
/// The Backup module never stores its own copy of these values — it always reads
/// them here, so there is a single source of truth.
/// </summary>
public sealed class BackupSettingsProvider : IBackupSettingsProvider
{
    private readonly ISettingsReadStore _readStore;
    private readonly IFolderResolver _resolver;

    public BackupSettingsProvider(ISettingsReadStore readStore, IFolderResolver resolver)
    {
        _readStore = readStore;
        _resolver = resolver;
    }

    public async Task<BackupConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var values = await _readStore.GetAllValuesAsync(cancellationToken);

        bool Bool(string key) =>
            string.Equals(values.TryGetValue(key, out var v) ? v : null, "true", StringComparison.OrdinalIgnoreCase);
        int Int(string key) =>
            int.TryParse(values.TryGetValue(key, out var v) ? v : null, out var n) ? n : 0;
        string Str(string key) =>
            values.TryGetValue(key, out var v) && v is not null ? v : string.Empty;

        var backupFolder = _resolver.Resolve(Str(SettingCatalog.Keys.StorageBackupFolder), "Backups");

        var contentFolders = new List<string>();
        foreach (var definition in SettingCatalog.FolderDefinitions)
        {
            if (definition.Key == SettingCatalog.Keys.StorageBackupFolder)
                continue;

            var path = _resolver.Resolve(values.TryGetValue(definition.Key, out var v) ? v : definition.DefaultValue, definition.FolderSubPath);
            if (Directory.Exists(path)) contentFolders.Add(path);
        }

        var applicationVersion = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0.0";

        return new BackupConfiguration
        {
            DatabasePath = App.DatabasePath,
            BackupFolder = backupFolder,
            ContentFolders = contentFolders,
            AutomaticBackup = Bool(SettingCatalog.Keys.BackupAutomaticBackup),
            Frequency = Str(SettingCatalog.Keys.BackupFrequency),
            Time = Str(SettingCatalog.Keys.BackupTime),
            MaximumCount = Int(SettingCatalog.Keys.BackupMaximumCount),
            Compress = Bool(SettingCatalog.Keys.BackupCompress),
            VerifyIntegrity = Bool(SettingCatalog.Keys.BackupVerifyIntegrity),
            ApplicationVersion = applicationVersion
        };
    }
}
