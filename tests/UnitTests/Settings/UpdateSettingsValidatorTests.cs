using FluentAssertions;
using RMS.Modules.Settings.Application.Models;
using RMS.Modules.Settings.Application.UpdateGeneralSettings;
using RMS.Modules.Settings.Application.UpdateReceiptSettings;
using RMS.Modules.Settings.Application.UpdateReportSettings;
using RMS.Modules.Settings.Application.UpdateSalesSettings;
using RMS.Modules.Settings.Application.UpdateStorageSettings;
using Xunit;

namespace RMS.UnitTests.Settings;

public class UpdateSettingsValidatorTests
{
    [Fact]
    public void General_WithEmptyStoreName_Should_Fail()
    {
        var cmd = new UpdateGeneralSettingsCommand(new GeneralSettingsModel { StoreName = "" });
        var result = new UpdateGeneralSettingsValidator().Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Settings.StoreName");
    }

    [Fact]
    public void General_WithValidData_Should_Pass()
    {
        var cmd = new UpdateGeneralSettingsCommand(new GeneralSettingsModel
        {
            StoreName = "Acme",
            Currency = "USD",
            Language = "English",
            DateFormat = "yyyy-MM-dd",
            TimeFormat = "HH:mm",
            Email = "a@b.com"
        });
        new UpdateGeneralSettingsValidator().Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Receipt_WithInvalidPaperWidth_Should_Fail()
    {
        var cmd = new UpdateReceiptSettingsCommand(new ReceiptSettingsModel { PaperWidth = 5 });
        new UpdateReceiptSettingsValidator().Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Sales_WithOutOfRangeRate_Should_Fail()
    {
        var cmd = new UpdateSalesSettingsCommand(new SalesSettingsModel { DefaultTaxRate = 150 });
        new UpdateSalesSettingsValidator().Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Report_WithInvalidOrientation_Should_Fail()
    {
        var cmd = new UpdateReportSettingsCommand(new ReportSettingsModel
        {
            DefaultReportFolder = @"C:\Reports",
            PrintOrientation = "Diagonal",
            PdfQuality = "Standard",
            ExcelExportFormat = "Xlsx",
            CsvDelimiter = ","
        });
        new UpdateReportSettingsValidator().Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Report_WithValidData_Should_Pass()
    {
        var cmd = new UpdateReportSettingsCommand(new ReportSettingsModel
        {
            DefaultReportFolder = @"C:\Reports",
            FileNamePattern = "{ReportType}_{yyyyMMdd}",
            PdfQuality = "Standard",
            CsvDelimiter = ",",
            ExcelExportFormat = "Xlsx",
            PrintOrientation = "Portrait"
        });
        new UpdateReportSettingsValidator().Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Storage_WithInvalidPath_Should_Fail()
    {
        var cmd = new UpdateStorageSettingsCommand(new[]
        {
            new FolderSettingModel { Key = "Storage.ReportsFolder", Path = "<<invalid>>" }
        });
        new UpdateStorageSettingsValidator().Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Storage_WithValidPath_Should_Pass()
    {
        var cmd = new UpdateStorageSettingsCommand(new[]
        {
            new FolderSettingModel { Key = "Storage.ReportsFolder", Path = @"C:\RMS\Reports" }
        });
        new UpdateStorageSettingsValidator().Validate(cmd).IsValid.Should().BeTrue();
    }
}
