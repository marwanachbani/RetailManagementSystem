using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RMS.Modules.Backup.Application.Models;

namespace RMS.Modules.Backup.Application;

/// <summary>
/// Pure, side-effect-light helpers for backup file naming, metadata (de)serialization
/// and integrity checks. Kept free of database/UI dependencies so they can be unit tested
/// in isolation and reused by the Backup service.
/// </summary>
public static class BackupFileHelper
{
    public const string MetadataFileName = "backup.metadata.json";
    public const string DatabaseFileName = "rms.db";
    public const string DataFolderName = "Data";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Deterministic base name for a backup, e.g. "backup-20260712-143015-3f2a...".</summary>
    public static string GenerateBackupBaseName(DateTime now, Guid id) =>
        $"backup-{now:yyyyMMdd-HHmmss}-{id:N}";

    /// <summary>A compressed backup artifact ends with .zip; otherwise it is a folder.</summary>
    public static bool IsCompressedBackup(string path) =>
        path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

    /// <summary>SHA-256 (hex) of a file's bytes.</summary>
    public static string ComputeSha256(string filePath)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha.ComputeHash(stream);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public static string SerializeMetadata(BackupMetadata metadata) =>
        JsonSerializer.Serialize(metadata, JsonOptions);

    public static BackupMetadata? DeserializeMetadata(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<BackupMetadata>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Reads the metadata file contained inside a backup (zip or folder).</summary>
    public static BackupMetadata? ReadMetadata(string backupPath)
    {
        if (IsCompressedBackup(backupPath))
        {
            using var archive = ZipFile.OpenRead(backupPath);
            var entry = archive.GetEntry(MetadataFileName)
                        ?? archive.Entries.FirstOrDefault(e => e.Name == MetadataFileName);
            if (entry is null) return null;
            using var reader = new StreamReader(entry.Open());
            return DeserializeMetadata(reader.ReadToEnd());
        }

        var metaPath = Path.Combine(backupPath, MetadataFileName);
        return File.Exists(metaPath) ? DeserializeMetadata(File.ReadAllText(metaPath)) : null;
    }

    /// <summary>Total size (bytes) of every file under a folder (recursive).</summary>
    public static long GetFolderSize(string folder)
    {
        if (!Directory.Exists(folder)) return 0;
        return Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
            .Sum(f => new FileInfo(f).Length);
    }
}
