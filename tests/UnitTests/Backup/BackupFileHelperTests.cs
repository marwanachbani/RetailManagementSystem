using System.IO.Compression;
using FluentAssertions;
using RMS.Modules.Backup.Application;
using RMS.Modules.Backup.Application.Models;
using Xunit;

namespace RMS.UnitTests.Backup;

public class BackupFileHelperTests
{
    [Fact]
    public void GenerateBackupBaseName_Should_IncludeTimestampAndId()
    {
        var now = new DateTime(2026, 7, 12, 14, 30, 15);
        var id = Guid.Parse("3f2a1b4c5d6e7f8091a2b3c4d5e6f708");
        var name = BackupFileHelper.GenerateBackupBaseName(now, id);

        name.Should().Be("backup-20260712-143015-3f2a1b4c5d6e7f8091a2b3c4d5e6f708");
    }

    [Theory]
    [InlineData("backup-20260712-143015-abc.zip", true)]
    [InlineData("backup-20260712-143015-abc", false)]
    [InlineData("C:\\Backups\\mybackup.zip", true)]
    [InlineData("C:\\Backups\\folder", false)]
    public void IsCompressedBackup_Should_DetectZipExtension(string path, bool expected)
    {
        BackupFileHelper.IsCompressedBackup(path).Should().Be(expected);
    }

    [Fact]
    public void ComputeSha256_Should_ReturnNonEmptyHash()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"rms_sha256_{Guid.NewGuid():N}.txt");
        File.WriteAllText(tempFile, "hello world");

        try
        {
            var hash = BackupFileHelper.ComputeSha256(tempFile);
            hash.Should().HaveLength(64);
            hash.Should().NotBeNullOrEmpty();
            hash.Should().MatchRegex("^[0-9a-f]{64}$");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetFolderSize_Should_ReturnZeroForMissingFolder()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"rms_missing_{Guid.NewGuid():N}");
        BackupFileHelper.GetFolderSize(missing).Should().Be(0);
    }

    [Fact]
    public void GetFolderSize_Should_SumAllFilesRecursively()
    {
        var root = Path.Combine(Path.GetTempPath(), $"rms_size_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "sub"));
        File.WriteAllBytes(Path.Combine(root, "a.txt"), new byte[100]);
        File.WriteAllBytes(Path.Combine(root, "sub", "b.txt"), new byte[200]);

        try
        {
            BackupFileHelper.GetFolderSize(root).Should().Be(300);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SerializeAndDeserializeMetadata_Should_RoundTrip()
    {
        var metadata = new BackupMetadata
        {
            BackupId = Guid.NewGuid(),
            FileName = "backup-test",
            Date = new DateTime(2026, 7, 12, 10, 0, 0),
            User = "admin",
            Size = 12345,
            ApplicationVersion = "1.0.0.0",
            DatabaseVersion = "5",
            Notes = "Test backup",
            Checksum = "abcd1234",
            Contents = new[] { "Images", "Reports" }
        };

        var json = BackupFileHelper.SerializeMetadata(metadata);
        var deserialized = BackupFileHelper.DeserializeMetadata(json);

        deserialized.Should().NotBeNull();
        deserialized!.BackupId.Should().Be(metadata.BackupId);
        deserialized.FileName.Should().Be(metadata.FileName);
        deserialized.User.Should().Be(metadata.User);
        deserialized.Checksum.Should().Be(metadata.Checksum);
        deserialized.Contents.Should().HaveCount(2);
    }

    [Fact]
    public void DeserializeMetadata_WithNullJson_Should_ReturnNull()
    {
        BackupFileHelper.DeserializeMetadata(null!).Should().BeNull();
        BackupFileHelper.DeserializeMetadata(string.Empty).Should().BeNull();
        BackupFileHelper.DeserializeMetadata("not json").Should().BeNull();
    }

    [Fact]
    public void ReadMetadata_Should_ReadFromUncompressedFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"rms_meta_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var metadata = new BackupMetadata { BackupId = Guid.NewGuid(), FileName = "test", Checksum = "cafe" };
        File.WriteAllText(Path.Combine(root, BackupFileHelper.MetadataFileName), BackupFileHelper.SerializeMetadata(metadata));
        File.WriteAllText(Path.Combine(root, BackupFileHelper.DatabaseFileName), "dummydb");

        try
        {
            var result = BackupFileHelper.ReadMetadata(root);
            result.Should().NotBeNull();
            result!.BackupId.Should().Be(metadata.BackupId);
            result.Checksum.Should().Be(metadata.Checksum);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ReadMetadata_Should_ReadFromCompressedZip()
    {
        var root = Path.Combine(Path.GetTempPath(), $"rms_zipmeta_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var source = Path.Combine(root, "source");
        Directory.CreateDirectory(source);

        var metadata = new BackupMetadata { BackupId = Guid.NewGuid(), FileName = "testzip", Checksum = "babe" };
        File.WriteAllText(Path.Combine(source, BackupFileHelper.MetadataFileName), BackupFileHelper.SerializeMetadata(metadata));
        File.WriteAllText(Path.Combine(source, BackupFileHelper.DatabaseFileName), "dummydb");

        var zipPath = Path.Combine(root, "backup.zip");
        ZipFile.CreateFromDirectory(source, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);

        try
        {
            var result = BackupFileHelper.ReadMetadata(zipPath);
            result.Should().NotBeNull();
            result!.BackupId.Should().Be(metadata.BackupId);
            result.Checksum.Should().Be(metadata.Checksum);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
