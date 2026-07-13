using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Printing.Application;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Application.Models;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Entities;
using RMS.Modules.Printing.Domain.Models;
using RMS.Modules.Settings.Domain;
using RMS.Modules.Settings.Domain.Entities;
using Xunit;

namespace RMS.IntegrationTests.Printing;

public class PrintJobRepositoryTests : PrintingIntegrationTestBase
{
    public PrintJobRepositoryTests(PrintingTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task AddAsync_Should_InsertPrintJob()
    {
        var job = PrintJob.Create(DocumentType.Receipt, "R-001", "Printer1");
        await PrintJobRepository.AddAsync(job);

        var recent = await PrintJobRepository.GetRecentAsync();
        recent.Should().HaveCount(1);
        recent[0].DocumentNumber.Should().Be("R-001");
        recent[0].PrinterName.Should().Be("Printer1");
        recent[0].Status.Should().Be(PrintJobStatus.Queued);
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateJobStatus()
    {
        var job = PrintJob.Create(DocumentType.Invoice, "INV-001", "Printer1");
        await PrintJobRepository.AddAsync(job);
        job.MarkCompleted("/tmp/invoice.pdf");
        await PrintJobRepository.UpdateAsync(job);

        var recent = await PrintJobRepository.GetRecentAsync();
        recent[0].Status.Should().Be(PrintJobStatus.Completed);
        recent[0].OutputPath.Should().Be("/tmp/invoice.pdf");
        recent[0].CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_RecordFailure()
    {
        var job = PrintJob.Create(DocumentType.BarcodeLabel, "LBL-001", "Printer1");
        await PrintJobRepository.AddAsync(job);
        job.MarkFailed("Paper empty");
        await PrintJobRepository.UpdateAsync(job);

        var recent = await PrintJobRepository.GetRecentAsync();
        recent[0].Status.Should().Be(PrintJobStatus.Failed);
        recent[0].ErrorMessage.Should().Be("Paper empty");
    }

    [Fact]
    public async Task GetRecentAsync_Should_ReturnJobsInDescendingOrder()
    {
        await PrintJobRepository.AddAsync(PrintJob.Create(DocumentType.Receipt, "R-001", "P1"));
        await PrintJobRepository.AddAsync(PrintJob.Create(DocumentType.Receipt, "R-002", "P1"));

        var recent = await PrintJobRepository.GetRecentAsync(10);
        recent.Should().HaveCount(2);
        recent[0].DocumentNumber.Should().Be("R-002");
        recent[1].DocumentNumber.Should().Be("R-001");
    }

    [Fact]
    public async Task GetRecentAsync_Should_RespectLimit()
    {
        for (var i = 0; i < 5; i++)
            await PrintJobRepository.AddAsync(PrintJob.Create(DocumentType.Receipt, $"R-{i:D3}", "P1"));

        var recent = await PrintJobRepository.GetRecentAsync(3);
        recent.Should().HaveCount(3);
    }
}

public class PrintSettingsProviderTests : PrintingIntegrationTestBase
{
    public PrintSettingsProviderTests(PrintingTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetAsync_Should_ReturnDefaultsWhenNoSettings()
    {
        var settings = await PrintSettingsProvider.GetAsync();
        settings.Should().NotBeNull();
        settings.DefaultPrinter.Should().NotBeNull();
        settings.Branding.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_Should_ReadPrinterSettingsFromStore()
    {
        var pairs = new Dictionary<string, string?>
        {
            [SettingCatalog.Keys.PrinterDefault] = "MyPrinter",
            [SettingCatalog.Keys.PrinterReceipt] = "ReceiptPrinter",
            [SettingCatalog.Keys.PrinterInvoice] = "InvoicePrinter",
            [SettingCatalog.Keys.PrinterLabel] = "LabelPrinter",
            [SettingCatalog.Keys.PrinterReport] = "ReportPrinter",
            [SettingCatalog.Keys.PrinterCopies] = "2",
            [SettingCatalog.Keys.PrinterPaperWidth] = "58",
            [SettingCatalog.Keys.PrinterOrientation] = "Landscape",
            [SettingCatalog.Keys.PrinterMarginMm] = "15",
            [SettingCatalog.Keys.PrinterCutPaper] = "true",
            [SettingCatalog.Keys.PrinterOpenDrawer] = "true",
        };
        await SettingsWriteStore.UpsertManyAsync(pairs);

        var settings = await PrintSettingsProvider.GetAsync();
        settings.DefaultPrinter.Should().Be("MyPrinter");
        settings.ReceiptPrinter.Should().Be("ReceiptPrinter");
        settings.InvoicePrinter.Should().Be("InvoicePrinter");
        settings.LabelPrinter.Should().Be("LabelPrinter");
        settings.ReportPrinter.Should().Be("ReportPrinter");
        settings.Copies.Should().Be(2);
        settings.PaperWidthMm.Should().Be(58);
        settings.Orientation.Should().Be(PrintOrientation.Landscape);
        settings.MarginMm.Should().Be(15);
        settings.CutPaper.Should().BeTrue();
        settings.OpenDrawer.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_Should_ReadBrandingFromStore()
    {
        var pairs = new Dictionary<string, string?>
        {
            [SettingCatalog.Keys.GeneralStoreName] = "My Store",
            [SettingCatalog.Keys.StoreCompanyAddress] = "123 Main St",
            [SettingCatalog.Keys.GeneralPhoneNumber] = "555-0100",
            [SettingCatalog.Keys.GeneralTaxNumber] = "TAX-123",
            [SettingCatalog.Keys.GeneralEmail] = "store@test.com",
            [SettingCatalog.Keys.GeneralWebsite] = "store.com",
            [SettingCatalog.Keys.ReceiptHeader] = "Header text",
            [SettingCatalog.Keys.ReceiptFooter] = "Footer text",
        };
        await SettingsWriteStore.UpsertManyAsync(pairs);

        var settings = await PrintSettingsProvider.GetAsync();
        settings.Branding.StoreName.Should().Be("My Store");
        settings.Branding.Address.Should().Be("123 Main St");
        settings.Branding.Phone.Should().Be("555-0100");
        settings.Branding.TaxNumber.Should().Be("TAX-123");
        settings.Branding.Email.Should().Be("store@test.com");
        settings.Branding.Website.Should().Be("store.com");
        settings.Branding.ReceiptHeader.Should().Be("Header text");
        settings.Branding.ReceiptFooter.Should().Be("Footer text");
    }
}

public class BarcodeGeneratorIntegrationTests : PrintingIntegrationTestBase
{
    public BarcodeGeneratorIntegrationTests(PrintingTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public void Generate_Code128_Should_ProducePngBytes()
    {
        var png = BarcodeGenerator.Generate("TEST-123", BarcodeSymbology.Code128, 200, 80);
        png.Should().NotBeEmpty();
        png.Length.Should().BeGreaterThan(50);
    }

    [Fact]
    public void GenerateQr_Should_ProducePngBytes()
    {
        var png = BarcodeGenerator.GenerateQr("https://example.com", 150);
        png.Should().NotBeEmpty();
        png.Length.Should().BeGreaterThan(50);
    }
}

public class RendererIntegrationTests : PrintingIntegrationTestBase
{
    public RendererIntegrationTests(PrintingTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public void Render_Receipt_Should_ProducePdf()
    {
        var receipt = new ReceiptDocument(
            ReceiptNumber: "R-001",
            SaleDate: DateTime.Now,
            CashierName: "John",
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(100, 0, 10, 110, 110, 0, "Cash"));

        var pdf = Renderer.Render(DocumentType.Receipt, receipt, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
        pdf[0].Should().Be(0x25);
    }

    [Fact]
    public void Render_Invoice_Should_ProducePdf()
    {
        var invoice = new InvoiceDocument(
            InvoiceNumber: "INV-001",
            IssueDate: DateTime.Now,
            Seller: new DocumentParty("Store"),
            Customer: new DocumentParty("Customer"),
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(1000, 0, 200, 1200));

        var pdf = Renderer.Render(DocumentType.Invoice, invoice, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }
}
