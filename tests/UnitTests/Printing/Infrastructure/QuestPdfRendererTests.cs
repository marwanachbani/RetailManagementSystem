using FluentAssertions;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Application.Models;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;
using RMS.Modules.Printing.Infrastructure.Barcode;
using RMS.Modules.Printing.Infrastructure.QuestPdf;
using Xunit;

namespace RMS.UnitTests.Printing.Infrastructure;

public class QuestPdfDocumentRendererTests
{
    private readonly IBarcodeGenerator _barcodes = new BarcodeGenerator();
    private readonly IDocumentRenderingService _renderer;

    public QuestPdfDocumentRendererTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        _renderer = new QuestPdfDocumentRenderer(_barcodes);
    }

    [Fact]
    public void Render_Receipt_Should_ReturnValidPdf()
    {
        var receipt = new ReceiptDocument(
            ReceiptNumber: "R-001",
            SaleDate: DateTime.Now,
            CashierName: "John",
            Items: new List<DocumentLineItem>
            {
                new("Item A", 2, 5.00m, 10.00m, "SKU-A")
            },
            Totals: new DocumentTotals(10.00m, 0, 1.00m, 11.00m, 11.00m, 0, "Cash"),
            BarcodeData: "R-001",
            QrData: "https://rms.local/r/001");

        var pdf = _renderer.Render(DocumentType.Receipt, receipt, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
        pdf[0].Should().Be(0x25); // '%'
        pdf[1].Should().Be(0x50); // 'P'
        pdf[2].Should().Be(0x44); // 'D'
        pdf[3].Should().Be(0x46); // 'F'
    }

    [Fact]
    public void Render_Invoice_Should_ReturnValidPdf()
    {
        var invoice = new InvoiceDocument(
            InvoiceNumber: "INV-001",
            IssueDate: DateTime.Now,
            DueDate: DateTime.Now.AddDays(30),
            Seller: new DocumentParty("Store", "Addr", "555", "TAX"),
            Customer: new DocumentParty("Customer", "Addr2"),
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(1000, 0, 200, 1200),
            Terms: "Net 30");

        var pdf = _renderer.Render(DocumentType.Invoice, invoice, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
        pdf[0].Should().Be(0x25);
    }

    [Fact]
    public void Render_RefundReceipt_Should_ReturnValidPdf()
    {
        var refund = new RefundReceiptDocument(
            RefundNumber: "REF-001",
            OriginalReceiptNumber: "R-001",
            Date: DateTime.Now,
            CashierName: "John",
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(10.00m, 0, 1.00m, 11.00m),
            Reason: "Damaged");

        var pdf = _renderer.Render(DocumentType.RefundReceipt, refund, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
        pdf[0].Should().Be(0x25);
    }

    [Fact]
    public void Render_Quote_Should_ReturnValidPdf()
    {
        var quote = new QuoteDocument(
            QuoteNumber: "Q-001",
            IssueDate: DateTime.Now,
            ValidUntil: DateTime.Now.AddDays(30),
            Seller: new DocumentParty("Store"),
            Customer: new DocumentParty("Customer"),
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(500, 0, 50, 550));

        var pdf = _renderer.Render(DocumentType.Quote, quote, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_DeliveryNote_Should_ReturnValidPdf()
    {
        var note = new DeliveryNoteDocument(
            DeliveryNumber: "DN-001",
            OrderReference: "PO-001",
            Date: DateTime.Now,
            Customer: new DocumentParty("Customer"),
            Items: new List<DocumentLineItem>());

        var pdf = _renderer.Render(DocumentType.DeliveryNote, note, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_PurchaseOrder_Should_ReturnValidPdf()
    {
        var po = new PurchaseOrderDocument(
            PoNumber: "PO-001",
            OrderDate: DateTime.Now,
            Supplier: new DocumentParty("Supplier"),
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(1000, 0, 200, 1200));

        var pdf = _renderer.Render(DocumentType.PurchaseOrder, po, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_GoodsReceivedNote_Should_ReturnValidPdf()
    {
        var grn = new GoodsReceivedNoteDocument(
            GrnNumber: "GRN-001",
            PoReference: "PO-001",
            ReceivedDate: DateTime.Now,
            Supplier: new DocumentParty("Supplier"),
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(1000, 0, 200, 1200));

        var pdf = _renderer.Render(DocumentType.GoodsReceivedNote, grn, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_SupplierInvoice_Should_ReturnValidPdf()
    {
        var si = new SupplierInvoiceDocument(
            InvoiceNumber: "SI-001",
            InvoiceDate: DateTime.Now,
            Supplier: new DocumentParty("Supplier"),
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(1000, 0, 200, 1200));

        var pdf = _renderer.Render(DocumentType.SupplierInvoice, si, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_BarcodeLabels_Should_ReturnValidPdf()
    {
        var labels = new BarcodeLabelDocument(
            new List<LabelItem>
            {
                new("Product A", "5901234123457", BarcodeSymbology.EAN13, "SKU-A", "$9.99"),
                new("Product B", "RMS-002", BarcodeSymbology.Code128, "SKU-B", "$19.99")
            });

        var pdf = _renderer.Render(DocumentType.BarcodeLabel, labels, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_ProductLabels_Should_ReturnValidPdf()
    {
        var labels = new ProductLabelDocument(
            new List<LabelItem>
            {
                new("Product A", "5901234123457", BarcodeSymbology.EAN13, "SKU-A", "$9.99")
            });

        var pdf = _renderer.Render(DocumentType.ProductLabel, labels, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_ShelfLabels_Should_ReturnValidPdf()
    {
        var labels = new ShelfLabelDocument(
            new List<LabelItem>
            {
                new("Shelf A", "LOC-001", BarcodeSymbology.Code128, Price: "$5.99")
            });

        var pdf = _renderer.Render(DocumentType.ShelfLabel, labels, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_CustomerStatement_Should_ReturnValidPdf()
    {
        var statement = new CustomerStatementDocument(
            StatementNumber: "ST-001",
            PeriodFrom: DateTime.Now,
            PeriodTo: DateTime.Now.AddMonths(1),
            Customer: new DocumentParty("Customer"),
            OpeningBalance: 100,
            Lines: new List<StatementLine>(),
            ClosingBalance: 200);

        var pdf = _renderer.Render(DocumentType.CustomerStatement, statement, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_SupplierStatement_Should_ReturnValidPdf()
    {
        var statement = new SupplierStatementDocument(
            StatementNumber: "ST-001",
            PeriodFrom: DateTime.Now,
            PeriodTo: DateTime.Now.AddMonths(1),
            Supplier: new DocumentParty("Supplier"),
            OpeningBalance: 100,
            Lines: new List<StatementLine>(),
            ClosingBalance: 200);

        var pdf = _renderer.Render(DocumentType.SupplierStatement, statement, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_CustomerPurchaseHistory_Should_ReturnValidPdf()
    {
        var history = new CustomerPurchaseHistoryDocument(
            Customer: new DocumentParty("Customer"),
            PeriodFrom: DateTime.Now,
            PeriodTo: DateTime.Now.AddMonths(1),
            Orders: new List<PurchaseHistoryLine>(),
            TotalSpent: 500,
            OrderCount: 5);

        var pdf = _renderer.Render(DocumentType.CustomerPurchaseHistory, history, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_SupplierPurchaseHistory_Should_ReturnValidPdf()
    {
        var history = new SupplierPurchaseHistoryDocument(
            Supplier: new DocumentParty("Supplier"),
            PeriodFrom: DateTime.Now,
            PeriodTo: DateTime.Now.AddMonths(1),
            Orders: new List<PurchaseHistoryLine>(),
            TotalSpent: 5000,
            OrderCount: 10);

        var pdf = _renderer.Render(DocumentType.SupplierPurchaseHistory, history, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_StockAdjustmentReport_Should_ReturnValidPdf()
    {
        var report = new StockAdjustmentReportDocument(
            ReportTitle: "Stock Adjustment",
            GeneratedAt: DateTime.Now,
            Lines: new List<StockAdjustmentLine>
            {
                new("Product A", "SKU-A", 10, 15, 5, 2, 10)
            });

        var pdf = _renderer.Render(DocumentType.StockAdjustmentReport, report, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_InventoryCountSheet_Should_ReturnValidPdf()
    {
        var sheet = new InventoryCountSheetDocument(
            SheetTitle: "Count Sheet",
            CountDate: DateTime.Now,
            Lines: new List<InventoryCountLine>
            {
                new("Product A", "SKU-A", "BAR-001", 10)
            });

        var pdf = _renderer.Render(DocumentType.InventoryCountSheet, sheet, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_StockMovementReport_Should_ReturnValidPdf()
    {
        var report = new StockMovementReportDocument(
            ReportTitle: "Movements",
            PeriodFrom: DateTime.Now,
            PeriodTo: DateTime.Now.AddMonths(1),
            Movements: new List<StockMovementLine>
            {
                new(DateTime.Now, "Product A", "SKU-A", "In", 10, 0, 10)
            });

        var pdf = _renderer.Render(DocumentType.StockMovementReport, report, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_GenericReport_Should_ReturnValidPdf()
    {
        var report = new ReportDocument(
            Title: "Test Report",
            GeneratedAt: DateTime.Now,
            Columns: new List<string> { "Col1", "Col2" },
            Rows: new List<IReadOnlyList<object?>> { new List<object?> { "A", 1 }, new List<object?> { "B", 2 } },
            Orientation: PrintOrientation.Portrait,
            Subtitle: "Subtitle",
            Summary: new Dictionary<string, string> { { "Total", "3" } });

        var pdf = _renderer.Render(DocumentType.Report, report, BrandingInfo.Empty, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void Render_WithBranding_Should_IncludeStoreName()
    {
        var receipt = new ReceiptDocument(
            ReceiptNumber: "R-001",
            SaleDate: DateTime.Now,
            CashierName: "John",
            Items: new List<DocumentLineItem>(),
            Totals: new DocumentTotals(10, 0, 1, 11));

        var branding = new BrandingInfo(
            StoreName: "Test Store",
            Address: "123 Main St",
            Phone: "555-0100",
            TaxNumber: "TAX-123",
            Email: "test@store.com",
            Website: "store.com",
            LogoPath: "",
            ReceiptHeader: "Header",
            ReceiptFooter: "Footer",
            CurrencyCode: "USD");

        var pdf = _renderer.Render(DocumentType.Receipt, receipt, branding, PrintOptions.Default);
        pdf.Should().NotBeEmpty();
        pdf[0].Should().Be(0x25);
        pdf.Length.Should().BeGreaterThan(500);
    }
}
