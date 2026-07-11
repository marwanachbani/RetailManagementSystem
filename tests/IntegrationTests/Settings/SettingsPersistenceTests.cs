using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.GetSettings;
using RMS.Modules.Settings.Application.Models;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateGeneralSettings;
using RMS.Modules.Settings.Application.UpdateStorageSettings;
using RMS.Modules.Settings.Domain;
using Xunit;

namespace RMS.IntegrationTests.Settings;

public class SettingsPersistenceTests : SettingsIntegrationTestBase, IClassFixture<SettingsTestDatabaseFixture>
{
    private readonly IMediator _mediator;

    public SettingsPersistenceTests(SettingsTestDatabaseFixture fixture) : base(fixture)
    {
        _mediator = fixture.Services.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task GetSettings_Should_ReturnCatalogDefaults()
    {
        var result = await _mediator.Send(new GetSettingsQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value!.General.StoreName.Should().Be("My Retail Store");
        result.Value.Receipt.PaperWidth.Should().Be(80);
        result.Value.Storage.Should().HaveCount(11);
    }

    [Fact]
    public async Task UpdateGeneralSettings_Should_PersistAndBeReadBack()
    {
        var update = await _mediator.Send(new UpdateGeneralSettingsCommand(new GeneralSettingsModel
        {
            StoreName = "Persisted Store",
            Currency = "EUR",
            Language = "English",
            DateFormat = "yyyy-MM-dd",
            TimeFormat = "HH:mm"
        }));

        update.IsSuccess.Should().BeTrue();

        var stored = await ReadStore.GetValueAsync(SettingCatalog.Keys.GeneralStoreName);
        stored.Should().Be("Persisted Store");
    }

    [Fact]
    public async Task UpdateStorageSettings_Should_PersistFolderAndCreateIt()
    {
        var baseDir = Fixture.Services.GetRequiredService<RMS.Modules.Settings.Application.Services.IFolderResolver>().BaseDirectory;
        var receiptsPath = Path.Combine(baseDir, "MyReceipts");

        var update = await _mediator.Send(new UpdateStorageSettingsCommand(new List<FolderSettingModel>
        {
            new() { Key = SettingCatalog.Keys.StorageReceiptsFolder, Path = receiptsPath }
        }));

        update.IsSuccess.Should().BeTrue();
        (await ReadStore.GetValueAsync(SettingCatalog.Keys.StorageReceiptsFolder)).Should().Be(receiptsPath);
        Directory.Exists(receiptsPath).Should().BeTrue();
    }

    [Fact]
    public async Task ResetSettings_Should_RestoreDefaults()
    {
        await _mediator.Send(new UpdateGeneralSettingsCommand(new GeneralSettingsModel
        {
            StoreName = "Changed",
            Currency = "EUR",
            Language = "English",
            DateFormat = "yyyy-MM-dd",
            TimeFormat = "HH:mm"
        }));

        var reset = await _mediator.Send(new ResetSettingsCommand());

        reset.IsSuccess.Should().BeTrue();
        reset.Value!.General.StoreName.Should().Be("My Retail Store");
    }

    [Fact]
    public async Task Settings_Should_PersistAcrossNewConnections()
    {
        await WriteStore.UpsertAsync(SettingCatalog.Keys.GeneralWebsite, "https://example.com");

        var reloaded = await ReadStore.GetValueAsync(SettingCatalog.Keys.GeneralWebsite);
        reloaded.Should().Be("https://example.com");
    }
}
