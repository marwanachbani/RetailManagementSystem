using FluentAssertions;
using RMS.Modules.Printing.Application.Models;
using RMS.Modules.Printing.Domain;
using Xunit;

namespace RMS.UnitTests.Printing.Application;

public class PrintOptionsTests
{
    [Fact]
    public void Default_Should_HaveSensibleValues()
    {
        var options = PrintOptions.Default;
        options.Copies.Should().Be(1);
        options.PaperSize.Should().Be(PaperSize.A4);
        options.Orientation.Should().Be(PrintOrientation.Portrait);
        options.Duplex.Should().BeFalse();
        options.Collate.Should().BeTrue();
        options.MarginMm.Should().Be(10);
        options.PaperWidthMm.Should().Be(80);
    }

    [Fact]
    public void CustomOptions_Should_StoreAllValues()
    {
        var options = new PrintOptions(2, PaperSize.Thermal80Mm, PrintOrientation.Portrait, true, true, 15, 80);
        options.Copies.Should().Be(2);
        options.PaperSize.Should().Be(PaperSize.Thermal80Mm);
        options.Orientation.Should().Be(PrintOrientation.Portrait);
        options.Duplex.Should().BeTrue();
        options.Collate.Should().BeTrue();
        options.MarginMm.Should().Be(15);
        options.PaperWidthMm.Should().Be(80);
    }
}

public class PrintSettingsTests
{
    [Fact]
    public void ResolvePrinterFor_Receipt_Should_UseReceiptPrinter()
    {
        var settings = new PrintSettings(
            DefaultPrinter: "Default",
            ReceiptPrinter: "ReceiptPrinter",
            InvoicePrinter: "InvoicePrinter",
            LabelPrinter: "LabelPrinter",
            ReportPrinter: "ReportPrinter",
            AutoPrint: true,
            Copies: 1,
            PaperWidthMm: 80,
            Orientation: PrintOrientation.Portrait,
            MarginMm: 10,
            CutPaper: true,
            OpenDrawer: false,
            Branding: new RMS.Modules.Printing.Domain.Models.BrandingInfo(
                StoreName: "Store", Address: "", Phone: "", TaxNumber: "", Email: "", Website: "",
                LogoPath: "", ReceiptHeader: "", ReceiptFooter: "", CurrencyCode: "USD"));

        settings.ResolvePrinterFor(DocumentType.Receipt).Should().Be("ReceiptPrinter");
    }

    [Fact]
    public void ResolvePrinterFor_Invoice_Should_UseInvoicePrinter()
    {
        var settings = new PrintSettings(
            DefaultPrinter: "Default",
            ReceiptPrinter: "ReceiptPrinter",
            InvoicePrinter: "InvoicePrinter",
            LabelPrinter: "LabelPrinter",
            ReportPrinter: "ReportPrinter",
            AutoPrint: true,
            Copies: 1,
            PaperWidthMm: 80,
            Orientation: PrintOrientation.Portrait,
            MarginMm: 10,
            CutPaper: true,
            OpenDrawer: false,
            Branding: new RMS.Modules.Printing.Domain.Models.BrandingInfo(
                StoreName: "Store", Address: "", Phone: "", TaxNumber: "", Email: "", Website: "",
                LogoPath: "", ReceiptHeader: "", ReceiptFooter: "", CurrencyCode: "USD"));

        settings.ResolvePrinterFor(DocumentType.Invoice).Should().Be("InvoicePrinter");
    }

    [Fact]
    public void ResolvePrinterFor_Label_Should_UseLabelPrinter()
    {
        var settings = new PrintSettings(
            DefaultPrinter: "Default",
            ReceiptPrinter: "ReceiptPrinter",
            InvoicePrinter: "InvoicePrinter",
            LabelPrinter: "LabelPrinter",
            ReportPrinter: "ReportPrinter",
            AutoPrint: true,
            Copies: 1,
            PaperWidthMm: 80,
            Orientation: PrintOrientation.Portrait,
            MarginMm: 10,
            CutPaper: true,
            OpenDrawer: false,
            Branding: new RMS.Modules.Printing.Domain.Models.BrandingInfo(
                StoreName: "Store", Address: "", Phone: "", TaxNumber: "", Email: "", Website: "",
                LogoPath: "", ReceiptHeader: "", ReceiptFooter: "", CurrencyCode: "USD"));

        settings.ResolvePrinterFor(DocumentType.BarcodeLabel).Should().Be("LabelPrinter");
    }

    [Fact]
    public void ResolvePrinterFor_Report_Should_FallbackToDefaultWhenReportPrinterEmpty()
    {
        var settings = new PrintSettings(
            DefaultPrinter: "Default",
            ReceiptPrinter: "ReceiptPrinter",
            InvoicePrinter: "InvoicePrinter",
            LabelPrinter: "LabelPrinter",
            ReportPrinter: "",
            AutoPrint: true,
            Copies: 1,
            PaperWidthMm: 80,
            Orientation: PrintOrientation.Portrait,
            MarginMm: 10,
            CutPaper: true,
            OpenDrawer: false,
            Branding: new RMS.Modules.Printing.Domain.Models.BrandingInfo(
                StoreName: "Store", Address: "", Phone: "", TaxNumber: "", Email: "", Website: "",
                LogoPath: "", ReceiptHeader: "", ReceiptFooter: "", CurrencyCode: "USD"));

        settings.ResolvePrinterFor(DocumentType.Report).Should().Be("Default");
    }

    [Fact]
    public void ResolvePrinterFor_PurchaseOrder_Should_UseInvoicePrinter()
    {
        var settings = new PrintSettings(
            DefaultPrinter: "Default",
            ReceiptPrinter: "ReceiptPrinter",
            InvoicePrinter: "InvoicePrinter",
            LabelPrinter: "LabelPrinter",
            ReportPrinter: "ReportPrinter",
            AutoPrint: true,
            Copies: 1,
            PaperWidthMm: 80,
            Orientation: PrintOrientation.Portrait,
            MarginMm: 10,
            CutPaper: true,
            OpenDrawer: false,
            Branding: new RMS.Modules.Printing.Domain.Models.BrandingInfo(
                StoreName: "Store", Address: "", Phone: "", TaxNumber: "", Email: "", Website: "",
                LogoPath: "", ReceiptHeader: "", ReceiptFooter: "", CurrencyCode: "USD"));

        settings.ResolvePrinterFor(DocumentType.PurchaseOrder).Should().Be("InvoicePrinter");
    }
}
