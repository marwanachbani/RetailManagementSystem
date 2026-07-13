using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Application.Models;

public sealed record PrintOptions(
    int Copies = 1,
    PaperSize PaperSize = PaperSize.A4,
    PrintOrientation Orientation = PrintOrientation.Portrait,
    bool Duplex = false,
    bool Collate = true,
    int MarginMm = 10,
    int PaperWidthMm = 80)
{
    public static readonly PrintOptions Default = new();
}

/// <summary>Resolved printer + branding configuration consumed by the rendering and printing pipeline.</summary>
public sealed record PrintSettings(
    string DefaultPrinter,
    string ReceiptPrinter,
    string InvoicePrinter,
    string LabelPrinter,
    string ReportPrinter,
    bool AutoPrint,
    int Copies,
    int PaperWidthMm,
    PrintOrientation Orientation,
    int MarginMm,
    bool CutPaper,
    bool OpenDrawer,
    BrandingInfo Branding)
{
    public string ResolvePrinterFor(DocumentType type) => type switch
    {
        DocumentType.Receipt or DocumentType.RefundReceipt => FirstSet(ReceiptPrinter, DefaultPrinter),
        DocumentType.Invoice or DocumentType.Quote or DocumentType.DeliveryNote => FirstSet(InvoicePrinter, DefaultPrinter),
        DocumentType.BarcodeLabel or DocumentType.ProductLabel or DocumentType.ShelfLabel => FirstSet(LabelPrinter, DefaultPrinter),
        DocumentType.Report or DocumentType.CustomerStatement or DocumentType.CustomerPurchaseHistory
            or DocumentType.SupplierStatement or DocumentType.SupplierPurchaseHistory
            or DocumentType.StockAdjustmentReport or DocumentType.InventoryCountSheet or DocumentType.StockMovementReport
            => FirstSet(ReportPrinter, DefaultPrinter),
        DocumentType.PurchaseOrder or DocumentType.GoodsReceivedNote or DocumentType.SupplierInvoice => FirstSet(InvoicePrinter, DefaultPrinter),
        _ => DefaultPrinter
    };

    private static string FirstSet(string primary, string fallback) =>
        string.IsNullOrWhiteSpace(primary) ? fallback : primary;
}
