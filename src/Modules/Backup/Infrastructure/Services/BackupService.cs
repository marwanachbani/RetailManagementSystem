using System.IO.Compression;
using Dapper;
using Microsoft.Data.Sqlite;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Backup.Application;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Application.IntegrationEvents;
using RMS.Modules.Backup.Application.Models;
using RMS.Modules.Backup.Domain.Entities;
using RMS.Modules.Backup.Infrastructure.Persistence;

namespace RMS.Modules.Backup.Infrastructure.Services;

/// <summary>
/// Production backup &amp; restore engine. Works fully offline against the local
/// SQLite database and the on-disk content folders configured by the Settings module.
/// </summary>
public sealed class BackupService : IBackupService
{
    private const string PreRestoreNotes = "Automatic backup before restore";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IBackupStore _store;
    private readonly IBackupSettingsProvider _settings;
    private readonly ICurrentUserContext _currentUser;
    private readonly IEventBus _eventBus;

    public BackupService(
        IDbConnectionFactory connectionFactory,
        IBackupStore store,
        IBackupSettingsProvider settings,
        ICurrentUserContext currentUser,
        IEventBus eventBus)
    {
        _connectionFactory = connectionFactory;
        _store = store;
        _settings = settings;
        _currentUser = currentUser;
        _eventBus = eventBus;
    }

    public async Task<BackupResult> CreateBackupAsync(
        string? notes,
        IProgress<BackupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var config = await _settings.GetConfigurationAsync(cancellationToken);
        EnsureDirectory(config.BackupFolder);

        var id = Guid.NewGuid();
        var now = DateTime.Now;
        var baseName = BackupFileHelper.GenerateBackupBaseName(now, id);
        var folder = Path.Combine(config.BackupFolder, baseName);
        var dbTarget = Path.Combine(folder, BackupFileHelper.DatabaseFileName);

        try
        {
            Directory.CreateDirectory(folder);
            EnsureDirectory(Path.Combine(folder, BackupFileHelper.DataFolderName));

            progress?.Report(new BackupProgress(Stage: "Preparing database", Percent: 5, Message: "Creating a consistent copy of the database…"));
            cancellationToken.ThrowIfCancellationRequested();

            VacuumDatabaseInto(config.DatabasePath, dbTarget);

            progress?.Report(new BackupProgress(Stage: "Copying data", Percent: 30, Message: "Copying settings, images and attachments…"));
            cancellationToken.ThrowIfCancellationRequested();

            var contents = CopyContentFolders(config.ContentFolders, Path.Combine(folder, BackupFileHelper.DataFolderName));

            progress?.Report(new BackupProgress(Stage: "Computing checksum", Percent: 55, Message: "Calculating integrity checksum…"));
            var checksum = BackupFileHelper.ComputeSha256(dbTarget);

            var metadata = new BackupMetadata
            {
                BackupId = id,
                FileName = baseName,
                Date = now,
                User = _currentUser.UserName ?? "System",
                Size = 0,
                ApplicationVersion = config.ApplicationVersion,
                DatabaseVersion = GetDatabaseVersion(),
                Notes = notes,
                Checksum = checksum,
                Contents = contents
            };
            await File.WriteAllTextAsync(Path.Combine(folder, BackupFileHelper.MetadataFileName), BackupFileHelper.SerializeMetadata(metadata), cancellationToken);

            string finalPath = folder;
            bool compressed = false;
            if (config.Compress)
            {
                progress?.Report(new BackupProgress(Stage: "Compressing", Percent: 75, Message: "Compressing backup into a ZIP archive…"));
                cancellationToken.ThrowIfCancellationRequested();
                var zipPath = folder + ".zip";
                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(folder, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
                Directory.Delete(folder, recursive: true);
                finalPath = zipPath;
                compressed = true;
            }

            var size = compressed ? new FileInfo(finalPath).Length : BackupFileHelper.GetFolderSize(finalPath);
            metadata.Size = size;
            if (!compressed)
                await File.WriteAllTextAsync(Path.Combine(finalPath, BackupFileHelper.MetadataFileName), BackupFileHelper.SerializeMetadata(metadata), cancellationToken);

            progress?.Report(new BackupProgress(Stage: "Finalizing", Percent: 92, Message: "Recording backup in history…"));
            await _store.InsertAsync(new BackupHistory(
                id, baseName, finalPath, now, size, metadata.User, config.ApplicationVersion, notes, checksum), cancellationToken);

            await _eventBus.PublishAsync(new BackupCreatedIntegrationEvent(id, baseName, size, metadata.User), cancellationToken);

            progress?.Report(new BackupProgress(Stage: "Completed", Percent: 100, Message: "Backup completed successfully."));
            return new BackupResult
            {
                BackupId = id,
                FileName = baseName,
                FilePath = finalPath,
                Size = size,
                Checksum = checksum,
                Compressed = compressed
            };
        }
        catch (Exception)
        {
            CleanupPartialArtifact(folder, folder + ".zip");
            throw;
        }
        finally
        {
            try { await CleanupOldBackupsAsync(cancellationToken); } catch { /* best effort */ }
        }
    }

    public async Task<BackupVerificationResult> VerifyBackupAsync(
        string backupPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(backupPath) || !File.Exists(backupPath) && !Directory.Exists(backupPath))
            return new BackupVerificationResult(IsValid: false, Error: "The backup file or folder could not be found.");

        var metadata = BackupFileHelper.ReadMetadata(backupPath);
        if (metadata is null)
            return new BackupVerificationResult(IsValid: false, Error: "Backup metadata is missing or corrupted.");

        string? tempDb = null;
        try
        {
            string dbSource;
            if (BackupFileHelper.IsCompressedBackup(backupPath))
            {
                var tempDir = Path.Combine(Path.GetTempPath(), $"rms_verify_{Guid.NewGuid():N}");
                Directory.CreateDirectory(tempDir);
                ZipFile.ExtractToDirectory(backupPath, tempDir);
                dbSource = Path.Combine(tempDir, BackupFileHelper.DatabaseFileName);
                tempDb = tempDir;
            }
            else
            {
                dbSource = Path.Combine(backupPath, BackupFileHelper.DatabaseFileName);
            }

            if (!File.Exists(dbSource))
                return new BackupVerificationResult(IsValid: false, Error: "The database file is missing from the backup.");

            var computed = BackupFileHelper.ComputeSha256(dbSource);
            if (!string.Equals(computed, metadata.Checksum, StringComparison.OrdinalIgnoreCase))
                return new BackupVerificationResult(IsValid: false, Error: "Checksum mismatch — the backup appears to be corrupted or tampered with.");

            if (!IsDatabaseIntact(dbSource))
                return new BackupVerificationResult(IsValid: false, Error: "SQLite integrity check failed for the backup database.");

            return new BackupVerificationResult(IsValid: true);
        }
        catch (Exception ex)
        {
            return new BackupVerificationResult(IsValid: false, Error: $"Verification failed: {ex.Message}");
        }
        finally
        {
            if (tempDb is not null)
            {
                try { Directory.Delete(tempDb, recursive: true); } catch { /* best effort */ }
            }
        }
    }

    public async Task<BackupMetadata?> GetBackupDetailsAsync(
        string backupPath,
        CancellationToken cancellationToken = default)
        => BackupFileHelper.ReadMetadata(backupPath);

    public async Task<BackupDashboard> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var config = await _settings.GetConfigurationAsync(cancellationToken);
        var history = await _store.GetAllAsync(cancellationToken);

        var last = history.Count > 0 ? history[0] : null;
        var next = BackupScheduleHelper.ComputeNextRun(
            last?.BackupDate,
            config.Frequency,
            ParseTime(config.Time),
            DateTime.Now);

        long totalSize = 0;
        foreach (var entry in history)
        {
            if (File.Exists(entry.FilePath)) totalSize += new FileInfo(entry.FilePath).Length;
            else if (Directory.Exists(entry.FilePath)) totalSize += BackupFileHelper.GetFolderSize(entry.FilePath);
        }

        return new BackupDashboard
        {
            LastBackupDate = last?.BackupDate,
            LastBackupFileName = last?.FileName,
            NextScheduledBackup = config.AutomaticBackup && next.HasValue && next.Value > DateTime.Now ? next : null,
            BackupFolder = config.BackupFolder,
            TotalBackups = history.Count,
            TotalSize = totalSize,
            AutomaticBackupEnabled = config.AutomaticBackup,
            Frequency = config.Frequency
        };
    }

    public async Task<IReadOnlyList<BackupHistoryEntry>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        var history = await _store.GetAllAsync(cancellationToken);
        return history.Select(h => new BackupHistoryEntry
        {
            Id = h.Id,
            FileName = h.FileName,
            FilePath = h.FilePath,
            BackupDate = h.BackupDate,
            Size = h.Size,
            UserName = h.UserName,
            Version = h.Version,
            Notes = h.Notes,
            Checksum = h.Checksum,
            Status = File.Exists(h.FilePath) || Directory.Exists(h.FilePath) ? "Available" : "Missing"
        }).ToList();
    }

    public async Task<Result> DeleteBackupAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _store.GetByIdAsync(id, cancellationToken);
        if (record is null)
            return Result.Failure("The selected backup could not be found.");

        CleanupPartialArtifact(record.FilePath, record.FilePath);
        await _store.DeleteAsync(id, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> CleanupOldBackupsAsync(CancellationToken cancellationToken = default)
    {
        var config = await _settings.GetConfigurationAsync(cancellationToken);
        if (config.MaximumCount < 1) return Result.Success();

        var history = await _store.GetAllAsync(cancellationToken); // ordered DESC (newest first)
        if (history.Count <= config.MaximumCount) return Result.Success();

        var toRemove = history.Skip(config.MaximumCount);
        foreach (var entry in toRemove)
        {
            CleanupPartialArtifact(entry.FilePath, entry.FilePath);
            await _store.DeleteAsync(entry.Id, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<RestoreResult> RestoreAsync(
        RestoreRequest request,
        IProgress<RestoreProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (request.VerifyBeforeRestore)
            {
                progress?.Report(new RestoreProgress(Stage: "Verifying backup", Percent: 10, Message: "Verifying backup integrity…"));
                var verification = await VerifyBackupAsync(request.BackupPath, cancellationToken);
                if (!verification.IsValid)
                    return new RestoreResult(Succeeded: false, Error: verification.Error);
            }

            progress?.Report(new RestoreProgress(Stage: "Backing up current database", Percent: 30, Message: "Creating a safety backup of the current database…"));
            var preRestore = await CreateBackupAsync(PreRestoreNotes, null, cancellationToken);

            progress?.Report(new RestoreProgress(Stage: "Preparing restore", Percent: 50, Message: "Extracting backup contents…"));
            var config = await _settings.GetConfigurationAsync(cancellationToken);

            var (dbSource, dataDir) = ExtractBackup(request.BackupPath);

            progress?.Report(new RestoreProgress(Stage: "Restoring database", Percent: 70, Message: "Restoring the database…"));
            ReplaceLiveDatabase(config.DatabasePath, dbSource);

            progress?.Report(new RestoreProgress(Stage: "Restoring files", Percent: 85, Message: "Restoring images, reports and attachments…"));
            RestoreContentFolders(config.ContentFolders, dataDir);

            await _eventBus.PublishAsync(new BackupRestoredIntegrationEvent(preRestore.BackupId, preRestore.FileName, _currentUser.UserName ?? "System"), cancellationToken);

            progress?.Report(new RestoreProgress(Stage: "Done", Percent: 100, Message: "Restore completed. The application will now restart."));
            return new RestoreResult(Succeeded: true, BackupPath: request.BackupPath, PreRestoreBackupPath: preRestore.FilePath);
        }
        catch (Exception ex)
        {
            return new RestoreResult(Succeeded: false, Error: FriendlyRestoreError(ex));
        }
    }

    // ----- internals -----------------------------------------------------

    private static void VacuumDatabaseInto(string sourceDb, string targetDb)
    {
        if (File.Exists(targetDb)) File.Delete(targetDb);
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = sourceDb,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        connection.Execute($"VACUUM INTO '{EscapeSqlString(targetDb)}';");
    }

    private static void ReplaceLiveDatabase(string liveDbPath, string sourceDbPath)
    {
        SqliteConnection.ClearAllPools();
        DeleteWithRetry(liveDbPath);
        DeleteWithRetry(liveDbPath + "-wal");
        DeleteWithRetry(liveDbPath + "-shm");

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = sourceDbPath,
            Mode = SqliteOpenMode.ReadOnly,
            ForeignKeys = true
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        connection.Execute($"VACUUM INTO '{EscapeSqlString(liveDbPath)}';");
    }

    private static (string DbSource, string DataDir) ExtractBackup(string backupPath)
    {
        if (BackupFileHelper.IsCompressedBackup(backupPath))
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"rms_restore_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(backupPath, tempDir);
            return (Path.Combine(tempDir, BackupFileHelper.DatabaseFileName), Path.Combine(tempDir, BackupFileHelper.DataFolderName));
        }

        return (Path.Combine(backupPath, BackupFileHelper.DatabaseFileName), Path.Combine(backupPath, BackupFileHelper.DataFolderName));
    }

    private static void RestoreContentFolders(IReadOnlyList<string> liveFolders, string dataDir)
    {
        if (!Directory.Exists(dataDir)) return;
        foreach (var live in liveFolders)
        {
            var leaf = new DirectoryInfo(live).Name;
            var source = Path.Combine(dataDir, leaf);
            if (!Directory.Exists(source)) continue;
            EnsureDirectory(live);
            CopyDirectory(source, live);
        }
    }

    private static IReadOnlyList<string> CopyContentFolders(IReadOnlyList<string> sourceFolders, string dataRoot)
    {
        var leaves = new List<string>();
        foreach (var source in sourceFolders)
        {
            if (!Directory.Exists(source)) continue;
            var leaf = new DirectoryInfo(source).Name;
            var destination = Path.Combine(dataRoot, leaf);
            EnsureDirectory(destination);
            CopyDirectory(source, destination);
            leaves.Add(leaf);
        }

        return leaves;
    }

    private static void CopyDirectory(string source, string destination)
    {
        EnsureDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(source))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
        }

        foreach (var dir in Directory.EnumerateDirectories(source))
        {
            var destinationDir = Path.Combine(destination, new DirectoryInfo(dir).Name);
            if (IsSameOrSubdirectory(dir, destinationDir))
                continue;

            CopyDirectory(dir, destinationDir);
        }
    }

    private static bool IsSameOrSubdirectory(string source, string potentialSubdirectory)
    {
        var sourceFullPath = Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var potentialFullPath = Path.GetFullPath(potentialSubdirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return potentialFullPath.StartsWith(sourceFullPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || potentialFullPath.Equals(sourceFullPath, StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureDirectory(string path)
    {
        if (!string.IsNullOrWhiteSpace(path)) Directory.CreateDirectory(path);
    }

    private static void CleanupPartialArtifact(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            try
            {
                if (File.Exists(path)) File.Delete(path);
                else if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
            }
            catch { /* best effort */ }
        }
    }

    private static void DeleteWithRetry(string path, int attempts = 5)
    {
        for (int i = 0; i < attempts; i++)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(50);
            }
        }
    }

    private static bool IsDatabaseIntact(string dbPath)
    {
        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadOnly
            }.ToString();
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            var result = connection.ExecuteScalar<string>("PRAGMA integrity_check;");
            return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private string GetDatabaseVersion()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var version = connection.ExecuteScalar<long?>(
                "SELECT MAX(Version) FROM VersionInfo;");
            return version?.ToString() ?? "0";
        }
        catch
        {
            return "0";
        }
    }

    private static TimeSpan ParseTime(string value)
    {
        return TimeSpan.TryParse(value, out var ts) ? ts : new TimeSpan(23, 0, 0);
    }

    private static string EscapeSqlString(string value) => value.Replace("'", "''");

    private static string FriendlyRestoreError(Exception ex) => ex switch
    {
        OperationCanceledException => "The restore operation was cancelled.",
        UnauthorizedAccessException => "Permission denied while writing the restored files. Run the application with the required permissions.",
        IOException => $"A file error occurred during restore: {ex.Message}",
        _ => $"Restore failed: {ex.Message}"
    };
}
