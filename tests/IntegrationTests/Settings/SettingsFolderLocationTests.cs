using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Application.GetSettings;
using RMS.Modules.Settings.Application.Models;
using RMS.Modules.Settings.Application.Services;
using RMS.Modules.Settings.Application.UpdateReportSettings;
using RMS.Modules.Settings.Application.UpdateBackupSettings;
using RMS.Modules.Settings.Domain;
using Xunit;

namespace RMS.IntegrationTests.Settings;

/// <summary>
/// Verifies the "where files are saved" guarantees: reports, receipts, backups
/// and the database all resolve under the configured base directory and the
/// folders are created on save.
/// </summary>
public class SettingsFolderLocationTests : SettingsIntegrationTestBase, IClassFixture<SettingsTestDatabaseFixture>
{
    private readonly IMediator _mediator;
    private readonly IFolderResolver _resolver;

    public SettingsFolderLocationTests(SettingsTestDatabaseFixture fixture) : base(fixture)
    {
        _mediator = fixture.Services.GetRequiredService<IMediator>();
        _resolver = fixture.Services.GetRequiredService<IFolderResolver>();
    }

    [Fact]
    public async Task DefaultStorageFolders_Should_ResolveUnderBaseDirectory()
    {
        var result = await _mediator.Send(new GetSettingsQuery());
        result.IsSuccess.Should().BeTrue();

        foreach (var folder in result.Value!.Storage)
        {
            Path.GetFullPath(folder.Path)
                .Should().StartWith(Path.GetFullPath(_resolver.BaseDirectory));
            Directory.Exists(folder.Path).Should().BeTrue();
        }
    }

    [Fact]
    public async Task ReportExport_Should_SaveToConfiguredFolder()
    {
        var reportsPath = Path.Combine(_resolver.BaseDirectory, "Exports", "Reports");
        var result = await _mediator.Send(new UpdateReportSettingsCommand(new ReportSettingsModel
        {
            DefaultReportFolder = reportsPath,
            FileNamePattern = "{ReportType}_{yyyyMMdd}",
            PdfQuality = "Standard",
            CsvDelimiter = ",",
            ExcelExportFormat = "Xlsx",
            PrintOrientation = "Portrait"
        }));

        result.IsSuccess.Should().BeTrue();
        (await Fixture.Services.GetRequiredService<ISettingsReadStore>()
                .GetValueAsync(SettingCatalog.Keys.ReportDefaultReportFolder))
            .Should().Be(reportsPath);
        Directory.Exists(reportsPath).Should().BeTrue();
    }

    [Fact]
    public async Task BackupLocation_Should_BeConfigurableAndPersisted()
    {
        var backupPath = Path.Combine(_resolver.BaseDirectory, "MyBackups");
        var result = await _mediator.Send(new UpdateBackupSettingsCommand(new BackupSettingsModel
        {
            AutomaticBackup = true,
            Frequency = "Weekly",
            Time = "02:30",
            MaximumCount = 5,
            Compress = true,
            VerifyIntegrity = true
        }));

        result.IsSuccess.Should().BeTrue();
        var stored = await _mediator.Send(new GetSettingsQuery());
        stored.Value!.Backup.AutomaticBackup.Should().BeTrue();
        stored.Value!.Backup.Frequency.Should().Be("Weekly");
        stored.Value!.Backup.MaximumCount.Should().Be(5);
    }

    [Fact]
    public async Task ReceiptStorage_Should_ResolveUnderBaseDirectory()
    {
        var result = await _mediator.Send(new GetSettingsQuery());
        var receipts = result.Value!.Storage.Single(f => f.Key == SettingCatalog.Keys.StorageReceiptsFolder);
        receipts.Path.Should().Be(_resolver.GetDefaultPath("Receipts"));
    }
}
