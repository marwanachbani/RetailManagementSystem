using FluentAssertions;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Entities;
using RMS.Modules.Printing.Domain.Models;
using Xunit;

namespace RMS.UnitTests.Printing.Domain;

public class EnumsTests
{
    [Fact]
    public void DocumentType_Should_ContainAllSupportedDocuments()
    {
        var values = Enum.GetValues<DocumentType>();
        values.Should().Contain(DocumentType.Receipt);
        values.Should().Contain(DocumentType.Invoice);
        values.Should().Contain(DocumentType.RefundReceipt);
        values.Should().Contain(DocumentType.Quote);
        values.Should().Contain(DocumentType.DeliveryNote);
        values.Should().Contain(DocumentType.PurchaseOrder);
        values.Should().Contain(DocumentType.GoodsReceivedNote);
        values.Should().Contain(DocumentType.SupplierInvoice);
        values.Should().Contain(DocumentType.StockAdjustmentReport);
        values.Should().Contain(DocumentType.InventoryCountSheet);
        values.Should().Contain(DocumentType.StockMovementReport);
        values.Should().Contain(DocumentType.BarcodeLabel);
        values.Should().Contain(DocumentType.ProductLabel);
        values.Should().Contain(DocumentType.ShelfLabel);
        values.Should().Contain(DocumentType.CustomerStatement);
        values.Should().Contain(DocumentType.CustomerPurchaseHistory);
        values.Should().Contain(DocumentType.SupplierStatement);
        values.Should().Contain(DocumentType.SupplierPurchaseHistory);
        values.Should().Contain(DocumentType.Report);
    }

    [Fact]
    public void PrinterKind_Should_HaveWindowsAndThermalPos()
    {
        var values = Enum.GetValues<PrinterKind>();
        values.Should().HaveCount(2);
        values.Should().Contain(PrinterKind.Windows);
        values.Should().Contain(PrinterKind.ThermalPos);
    }

    [Fact]
    public void PaperSize_Should_ContainThermalAndA4()
    {
        var values = Enum.GetValues<PaperSize>();
        values.Should().Contain(PaperSize.Thermal58Mm);
        values.Should().Contain(PaperSize.Thermal80Mm);
        values.Should().Contain(PaperSize.A4);
    }

    [Fact]
    public void BarcodeSymbology_Should_ContainEan13Code128Code39QrCode()
    {
        var values = Enum.GetValues<BarcodeSymbology>();
        values.Should().Contain(BarcodeSymbology.EAN13);
        values.Should().Contain(BarcodeSymbology.Code128);
        values.Should().Contain(BarcodeSymbology.Code39);
        values.Should().Contain(BarcodeSymbology.QRCode);
    }

    [Fact]
    public void PrintJobStatus_Should_HaveAllStates()
    {
        var values = Enum.GetValues<PrintJobStatus>();
        values.Should().Contain(PrintJobStatus.Queued);
        values.Should().Contain(PrintJobStatus.Printing);
        values.Should().Contain(PrintJobStatus.Completed);
        values.Should().Contain(PrintJobStatus.Failed);
        values.Should().Contain(PrintJobStatus.Cancelled);
    }

    [Fact]
    public void PrinterStatus_Should_HaveAllStates()
    {
        var values = Enum.GetValues<PrinterStatus>();
        values.Should().Contain(PrinterStatus.Ready);
        values.Should().Contain(PrinterStatus.Offline);
        values.Should().Contain(PrinterStatus.OutOfPaper);
        values.Should().Contain(PrinterStatus.Error);
        values.Should().Contain(PrinterStatus.Unknown);
    }
}

public class PrinterDescriptorTests
{
    [Fact]
    public void WindowsPrinter_Should_NotBeThermal()
    {
        var descriptor = new PrinterDescriptor("HP LaserJet", PrinterKind.Windows, false, PrinterStatus.Ready);
        descriptor.IsThermal.Should().BeFalse();
    }

    [Fact]
    public void ThermalPrinter_Should_BeThermal()
    {
        var descriptor = new PrinterDescriptor("EPSON TM-T88V", PrinterKind.ThermalPos, false, PrinterStatus.Ready, 80);
        descriptor.IsThermal.Should().BeTrue();
    }

    [Fact]
    public void DefaultPrinter_Should_MarkIsDefault()
    {
        var descriptor = new PrinterDescriptor("Default Printer", PrinterKind.Windows, true, PrinterStatus.Ready);
        descriptor.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Thermal58Mm_Should_HaveCorrectPaperWidth()
    {
        var descriptor = new PrinterDescriptor("58mm Printer", PrinterKind.ThermalPos, false, PrinterStatus.Ready, 58);
        descriptor.PaperWidthMm.Should().Be(58);
    }

    [Fact]
    public void DefaultPaperWidth_Should_Be80()
    {
        var descriptor = new PrinterDescriptor("Printer", PrinterKind.Windows, false, PrinterStatus.Ready);
        descriptor.PaperWidthMm.Should().Be(80);
    }
}

public class PrintingExceptionsTests
{
    [Fact]
    public void PrinterNotFoundException_Should_CarryErrorCode()
    {
        var ex = new PrinterNotFoundException("TestPrinter");
        ex.ErrorCode.Should().Be("PRINTER_NOT_FOUND");
        ex.Message.Should().Contain("TestPrinter");
    }

    [Fact]
    public void PrinterOfflineException_Should_CarryErrorCode()
    {
        var ex = new PrinterOfflineException("TestPrinter");
        ex.ErrorCode.Should().Be("PRINTER_OFFLINE");
    }

    [Fact]
    public void PaperEmptyException_Should_CarryErrorCode()
    {
        var ex = new PaperEmptyException("TestPrinter");
        ex.ErrorCode.Should().Be("PRINTER_NO_PAPER");
    }

    [Fact]
    public void InvalidPrinterException_Should_CarryErrorCode()
    {
        var ex = new InvalidPrinterException("bad printer");
        ex.ErrorCode.Should().Be("PRINTER_INVALID");
    }

    [Fact]
    public void PrintAccessException_Should_CarryErrorCode()
    {
        var ex = new PrintAccessException("TestPrinter", "detail");
        ex.ErrorCode.Should().Be("PRINTER_ACCESS_DENIED");
    }

    [Fact]
    public void PrintFailureException_Should_CarryErrorCode()
    {
        var ex = new PrintFailureException("TestPrinter", "detail");
        ex.ErrorCode.Should().Be("PRINT_FAILED");
    }

    [Fact]
    public void AllExceptions_Should_InheritFromPrintingException()
    {
        typeof(PrinterNotFoundException).Should().BeDerivedFrom<PrintingException>();
        typeof(PrinterOfflineException).Should().BeDerivedFrom<PrintingException>();
        typeof(PaperEmptyException).Should().BeDerivedFrom<PrintingException>();
        typeof(InvalidPrinterException).Should().BeDerivedFrom<PrintingException>();
        typeof(PrintAccessException).Should().BeDerivedFrom<PrintingException>();
        typeof(PrintFailureException).Should().BeDerivedFrom<PrintingException>();
    }
}

public class PrintJobTests
{
    [Fact]
    public void Create_Should_InitializeWithDefaults()
    {
        var job = PrintJob.Create(DocumentType.Receipt, "R-001", "Printer1");
        job.Id.Should().NotBe(Guid.Empty);
        job.DocumentType.Should().Be("Receipt");
        job.DocumentNumber.Should().Be("R-001");
        job.PrinterName.Should().Be("Printer1");
        job.Status.Should().Be(PrintJobStatus.Queued);
        job.Copies.Should().Be(1);
    }

    [Fact]
    public void MarkCompleted_Should_SetStatusAndTimestamp()
    {
        var job = PrintJob.Create(DocumentType.Invoice, "INV-001", "Printer1");
        job.MarkCompleted("/tmp/test.pdf");
        job.Status.Should().Be(PrintJobStatus.Completed);
        job.CompletedAt.Should().NotBeNull();
        job.OutputPath.Should().Be("/tmp/test.pdf");
    }

    [Fact]
    public void MarkFailed_Should_SetStatusAndError()
    {
        var job = PrintJob.Create(DocumentType.Receipt, "R-001", "Printer1");
        job.MarkFailed("Out of paper");
        job.Status.Should().Be(PrintJobStatus.Failed);
        job.ErrorMessage.Should().Be("Out of paper");
        job.CompletedAt.Should().NotBeNull();
    }
}

public class DocumentModelsTests
{
    [Fact]
    public void ReceiptDocument_Should_StoreAllFields()
    {
        var receipt = new ReceiptDocument(
            ReceiptNumber: "R-001",
            SaleDate: DateTime.Now,
            CashierName: "John",
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(100, 0, 10, 110, 110, 0, "Cash"),
            CustomerName: "Jane",
            BarcodeData: "R-001",
            QrData: "https://example.com/receipt/R-001",
            ThankYouMessage: "Thanks!",
            FooterText: "Footer");

        receipt.ReceiptNumber.Should().Be("R-001");
        receipt.CashierName.Should().Be("John");
        receipt.CustomerName.Should().Be("Jane");
        receipt.BarcodeData.Should().Be("R-001");
        receipt.QrData.Should().Be("https://example.com/receipt/R-001");
        receipt.ThankYouMessage.Should().Be("Thanks!");
        receipt.FooterText.Should().Be("Footer");
        receipt.Totals.GrandTotal.Should().Be(110);
    }

    [Fact]
    public void InvoiceDocument_Should_StorePartiesAndTotals()
    {
        var invoice = new InvoiceDocument(
            InvoiceNumber: "INV-001",
            IssueDate: DateTime.Now,
            DueDate: DateTime.Now.AddDays(30),
            Seller: new DocumentParty("Store", "Addr", "555", "TAX", "email"),
            Customer: new DocumentParty("Customer", "Addr2"),
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(1000, 0, 200, 1200),
            PoReference: "PO-001",
            Terms: "Net 30");

        invoice.InvoiceNumber.Should().Be("INV-001");
        invoice.Seller.Name.Should().Be("Store");
        invoice.Customer.Name.Should().Be("Customer");
        invoice.Totals.GrandTotal.Should().Be(1200);
        invoice.PoReference.Should().Be("PO-001");
        invoice.Terms.Should().Be("Net 30");
    }

    [Fact]
    public void LabelItem_Should_StoreAllProperties()
    {
        var label = new LabelItem("Product", "12345", BarcodeSymbology.Code128, "SKU-001", "$9.99", "Extra");
        label.Name.Should().Be("Product");
        label.BarcodeValue.Should().Be("12345");
        label.Symbology.Should().Be(BarcodeSymbology.Code128);
        label.Sku.Should().Be("SKU-001");
        label.Price.Should().Be("$9.99");
        label.ExtraLine.Should().Be("Extra");
    }

    [Fact]
    public void LabelLayout_Should_CalculateLabelsPerPage()
    {
        var layout = new LabelLayout(3, 8, 60, 35);
        layout.LabelsPerPage.Should().Be(24);
    }

    [Fact]
    public void CustomerStatementDocument_Should_StoreLines()
    {
        var statement = new CustomerStatementDocument(
            StatementNumber: "ST-001",
            PeriodFrom: DateTime.Now,
            PeriodTo: DateTime.Now.AddMonths(1),
            Customer: new DocumentParty("Customer"),
            OpeningBalance: 100,
            Lines: new List<StatementLine>(),
            ClosingBalance: 200);

        statement.StatementNumber.Should().Be("ST-001");
        statement.OpeningBalance.Should().Be(100);
        statement.ClosingBalance.Should().Be(200);
    }

    [Fact]
    public void StockAdjustmentReportDocument_Should_CalculateTotals()
    {
        var doc = new StockAdjustmentReportDocument(
            ReportTitle: "Adjustment",
            GeneratedAt: DateTime.Now,
            Lines: new List<StockAdjustmentLine>
            {
                new("Product A", "SKU-A", 10, 15, 5, 2, 10),
                new("Product B", "SKU-B", 20, 10, -10, 3, -30)
            });

        doc.TotalAdjustedQuantity.Should().Be(-5);
        doc.TotalAdjustedValue.Should().Be(-20);
    }

    [Fact]
    public void BrandingInfo_Should_DetectMissingLogo()
    {
        var branding = BrandingInfo.Empty;
        branding.HasLogo.Should().BeFalse();
        branding.StoreName.Should().Be("My Retail Store");
    }
}
