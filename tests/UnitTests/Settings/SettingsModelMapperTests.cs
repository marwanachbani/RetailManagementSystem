using FluentAssertions;
using RMS.Modules.Settings.Application;
using RMS.Modules.Settings.Application.Models;
using RMS.Modules.Settings.Application.Services;
using RMS.Modules.Settings.Domain;
using Xunit;

namespace RMS.UnitTests.Settings;

public class SettingsModelMapperTests
{
    private static FolderResolver Resolver(string baseDir) => new(baseDir);

    [Fact]
    public void ToModel_WithEmptyStore_Should_FallBackToDefaults()
    {
        var model = SettingsModelMapper.ToModel(new Dictionary<string, string?>(), Resolver("C:\\RMS"));

        model.General.StoreName.Should().Be("My Retail Store");
        model.Receipt.PaperWidth.Should().Be(80);
        model.Sales.MaximumDiscount.Should().Be(100);
        model.Backup.Frequency.Should().Be("Daily");
        model.Application.Theme.Should().Be("Light");
    }

    [Fact]
    public void ToModel_Should_ResolveStorageFoldersAgainstBaseDirectory()
    {
        var model = SettingsModelMapper.ToModel(new Dictionary<string, string?>(), Resolver(@"C:\RMS"));

        var reports = model.Storage.Single(f => f.Key == SettingCatalog.Keys.StorageReportsFolder);
        reports.Path.Should().Be(Path.Combine(@"C:\RMS", "Reports"));
        reports.DefaultPath.Should().Be(Path.Combine(@"C:\RMS", "Reports"));
    }

    [Fact]
    public void ToModel_Should_OverrideDefaultWithStoredValue()
    {
        var values = new Dictionary<string, string?>
        {
            [SettingCatalog.Keys.GeneralStoreName] = "Custom Store",
            [SettingCatalog.Keys.ReceiptShowQrCode] = "true",
            [SettingCatalog.Keys.SalesDefaultTaxRate] = "15"
        };

        var model = SettingsModelMapper.ToModel(values, Resolver("C:\\RMS"));

        model.General.StoreName.Should().Be("Custom Store");
        model.Receipt.ShowQrCode.Should().BeTrue();
        model.Sales.DefaultTaxRate.Should().Be(15);
        // Untouched keys keep the catalog default.
        model.Receipt.PaperWidth.Should().Be(80);
    }

    [Fact]
    public void PairsRoundTrip_Should_PreserveValues()
    {
        var original = new SettingsModel
        {
            General = new GeneralSettingsModel { StoreName = "Acme", Currency = "EUR" },
            Receipt = new ReceiptSettingsModel { PaperWidth = 58, ShowCashier = false, ShowBarcode = true }
        };

        var pairs = SettingsModelMapper.GeneralPairs(original.General);
        var receiptPairs = SettingsModelMapper.ReceiptPairs(original.Receipt);

        var combined = new Dictionary<string, string?>(pairs);
        foreach (var kv in receiptPairs) combined[kv.Key] = kv.Value;

        var restored = SettingsModelMapper.ToModel(combined, Resolver("C:\\RMS"));

        restored.General.StoreName.Should().Be("Acme");
        restored.General.Currency.Should().Be("EUR");
        restored.Receipt.PaperWidth.Should().Be(58);
        restored.Receipt.ShowCashier.Should().BeFalse();
        restored.Receipt.ShowBarcode.Should().BeTrue(); // default
    }

    [Fact]
    public void ReportPairs_Should_StoreFolderAsRelativeOrAbsolute()
    {
        var model = new ReportSettingsModel { DefaultReportFolder = @"C:\RMS\MyReports" };
        var pairs = SettingsModelMapper.ReportPairs(model, Resolver(@"C:\RMS"));
        pairs[SettingCatalog.Keys.ReportDefaultReportFolder].Should().Be(@"C:\RMS\MyReports");
    }
}
