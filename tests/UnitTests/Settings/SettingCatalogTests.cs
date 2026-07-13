using FluentAssertions;
using RMS.Modules.Settings.Domain;
using Xunit;

namespace RMS.UnitTests.Settings;

public class SettingCatalogTests
{
    [Fact]
    public void Catalog_Should_ContainElevenCategories()
    {
        SettingCatalog.Categories.Should().HaveCount(11);
        SettingCatalog.Categories.Select(c => c.Name).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Catalog_Should_DefineAllExpectedSettings()
    {
        var keys = SettingCatalog.Defaults.Select(d => d.Key).ToList();

        keys.Should().Contain(SettingCatalog.Keys.GeneralStoreName);
        keys.Should().Contain(SettingCatalog.Keys.ReceiptPaperWidth);
        keys.Should().Contain(SettingCatalog.Keys.SalesDefaultTaxRate);
        keys.Should().Contain(SettingCatalog.Keys.InventoryDefaultLowStockThreshold);
        keys.Should().Contain(SettingCatalog.Keys.PurchasingPurchaseNumberPrefix);
        keys.Should().Contain(SettingCatalog.Keys.ReportDefaultReportFolder);
        keys.Should().Contain(SettingCatalog.Keys.StorageReceiptsFolder);
        keys.Should().Contain(SettingCatalog.Keys.BackupAutomaticBackup);
        keys.Should().Contain(SettingCatalog.Keys.ApplicationTheme);
    }

    [Fact]
    public void Catalog_Should_HaveUniqueSettingKeys()
    {
        SettingCatalog.Defaults.Select(d => d.Key).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Catalog_Should_MarkStorageSettingsAsFolders()
    {
        var folderKeys = SettingCatalog.FolderDefinitions.Select(f => f.Key).ToList();
        folderKeys.Should().HaveCount(11);
        folderKeys.Should().OnlyContain(k => SettingCatalog.GetDefinition(k).IsFolder);
    }

    [Fact]
    public void Catalog_Should_AssignEachSettingToAKnownCategory()
    {
        var categoryNames = SettingCatalog.Categories.Select(c => c.Name).ToHashSet();
        SettingCatalog.Defaults.Should().OnlyContain(d => categoryNames.Contains(d.Category));
    }
}
