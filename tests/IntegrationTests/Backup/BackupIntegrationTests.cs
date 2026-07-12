using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Backup.Application;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Application.Models;
using RMS.Modules.Backup.Domain.Entities;
using RMS.Modules.Backup.Infrastructure.Persistence;
using Xunit;

namespace RMS.IntegrationTests.Backup;

public class BackupIntegrationTests : IClassFixture<BackupTestDatabaseFixture>
{
    private readonly BackupTestDatabaseFixture _fixture;
    private readonly IBackupService _service;
    private readonly IBackupStore _store;

    public BackupIntegrationTests(BackupTestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _service = fixture.Services.GetRequiredService<IBackupService>();
        _store = fixture.Services.GetRequiredService<IBackupStore>();
        _fixture.ResetState();
    }

    [Fact]
    public async Task CreateBackupAsync_Should_CreateBackupAndRecordHistory()
    {
        var result = await _service.CreateBackupAsync("Test backup");

        result.BackupId.Should().NotBeEmpty();
        result.FileName.Should().NotBeNullOrEmpty();
        result.FilePath.Should().NotBeNullOrEmpty();
        result.Size.Should().BeGreaterThan(0);
        result.Checksum.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateBackupAsync_Should_ReportProgress()
    {
        var progress = new List<BackupProgress>();
        var reporter = new Progress<BackupProgress>(p => progress.Add(p));

        var result = await _service.CreateBackupAsync("Progress test", reporter);

        result.FileName.Should().NotBeNullOrEmpty();
        progress.Should().NotBeEmpty();
        progress.Should().Contain(p => p.Stage == "Completed");
        progress.Should().Contain(p => p.Percent == 100);
        int prev = -1;
        foreach (var p in progress)
        {
            p.Percent.Should().BeGreaterOrEqualTo(prev);
            prev = p.Percent;
        }
    }

    [Fact]
    public async Task CreateBackupAsync_Should_SupportCancellation()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = async () => await _service.CreateBackupAsync("Cancel", null, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetHistoryAsync_Should_ReturnCreatedBackup()
    {
        await _service.CreateBackupAsync("History test");

        var history = await _service.GetHistoryAsync();

        history.Should().NotBeEmpty();
        history[0].FileName.Should().StartWith("backup-");
        history[0].UserName.Should().Be("System");
        history[0].Version.Should().NotBeNullOrEmpty();
        history[0].Checksum.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task VerifyBackupAsync_Should_ReturnValidForExistingBackup()
    {
        var created = await _service.CreateBackupAsync("Verify test");

        var result = await _service.VerifyBackupAsync(created.FilePath);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyBackupAsync_Should_ReturnInvalidForMissingFile()
    {
        var result = await _service.VerifyBackupAsync("C:\\nonexistent\\backup.zip");

        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetDashboardAsync_Should_ReturnCorrectSummary()
    {
        await _service.CreateBackupAsync("Dashboard test 1");
        await _service.CreateBackupAsync("Dashboard test 2");

        var dashboard = await _service.GetDashboardAsync();

        dashboard.TotalBackups.Should().Be(2);
        dashboard.LastBackupDate.Should().NotBeNull();
        dashboard.LastBackupFileName.Should().NotBeNullOrEmpty();
        dashboard.AutomaticBackupEnabled.Should().BeFalse();
        dashboard.BackupFolder.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteBackupAsync_Should_RemoveBackup()
    {
        var created = await _service.CreateBackupAsync("Delete test");

        var historyBefore = await _service.GetHistoryAsync();
        historyBefore.Should().HaveCount(1);

        var result = await _service.DeleteBackupAsync(created.BackupId);
        result.IsSuccess.Should().BeTrue();

        var historyAfter = await _service.GetHistoryAsync();
        historyAfter.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteBackupAsync_Should_FailForMissingBackup()
    {
        var result = await _service.DeleteBackupAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateMultipleBackups_EachShouldHaveUniqueChecksum()
    {
        var b1 = await _service.CreateBackupAsync("Unique 1");
        await Task.Delay(1100);
        var b2 = await _service.CreateBackupAsync("Unique 2");

        b1.Checksum.Should().NotBe(b2.Checksum);
        b1.FileName.Should().NotBe(b2.FileName);
    }

    [Fact]
    public async Task BackupShould_IncludeMetadataFile()
    {
        var created = await _service.CreateBackupAsync("Metadata test");
        var zipPath = created.FilePath;

        using (var archive = System.IO.Compression.ZipFile.OpenRead(zipPath))
        {
            var metaEntry = archive.GetEntry("backup.metadata.json");
            metaEntry.Should().NotBeNull();

            using (var reader = new StreamReader(metaEntry!.Open()))
            {
                var json = reader.ReadToEnd();
                json.Should().Contain("backup-2026");
                json.Should().Contain("System");
            }
        }
    }

    [Fact]
    public async Task GetBackupDetailsAsync_Should_ReadMetadata()
    {
        var created = await _service.CreateBackupAsync("Details test");

        var details = await _service.GetBackupDetailsAsync(created.FilePath);

        details.Should().NotBeNull();
        details!.BackupId.Should().Be(created.BackupId);
        details.FileName.Should().Be(created.FileName);
        details.Checksum.Should().Be(created.Checksum);
        details.ApplicationVersion.Should().NotBeNullOrEmpty();
    }
}
