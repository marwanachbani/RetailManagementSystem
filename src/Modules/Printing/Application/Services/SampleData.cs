using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Application.Services;

/// <summary>Factory for demo documents used by the "Test Print" actions.</summary>
public static class SampleData
{
    public static ReceiptDocument SampleReceipt() => new(
        ReceiptNumber: "R-100245",
        SaleDate: DateTime.Now,
        CashierName: "Jane Doe",
        CustomerName: "Walk-in Customer",
        Items: new List<DocumentLineItem>
        {
            new("Organic Bananas", 2, 1.20m, 2.40m, "BAN-001", "Per kg"),
            new("Whole Milk 1L", 3, 0.95m, 2.85m, "MLK-200"),
            new("Sourdough Loaf", 1, 3.50m, 3.50m, "BRD-010"),
            new("Sparkling Water", 4, 0.75m, 3.00m, "WTR-330", Discount: 0.40m)
        },
        Totals: new DocumentTotals(11.75m, 0.40m, 1.76m, 13.11m, PaidAmount: 20.00m, Change: 6.89m, PaymentMethod: "Cash"),
        BarcodeData: "R-100245",
        QrData: "https://rms.local/receipt/R-100245",
        ThankYouMessage: "Thank you for shopping with us!");

    public static InvoiceDocument SampleInvoice() => new(
        InvoiceNumber: "INV-2024-0042",
        IssueDate: DateTime.Now,
        DueDate: DateTime.Now.AddDays(30),
        Seller: new DocumentParty("My Retail Store", "123 Market Street", "555-0100", "GB123456789", "sales@rms.local", AccountNumber: "ACC-001"),
        Customer: new DocumentParty("Acme Trading Ltd", "88 Commerce Rd", "555-0200", "GB987654321", "ap@acme.com", AccountNumber: "ACM-7741", Balance: 0),
        Items: new List<DocumentLineItem>
        {
            new("Wireless Mouse", 25, 12.00m, 300.00m, "ELC-501", Unit: "pcs"),
            new("USB-C Hub", 10, 28.50m, 285.00m, "ELC-512", Unit: "pcs"),
            new("HDMI Cable 2m", 40, 6.25m, 250.00m, "ELC-530", Unit: "pcs")
        },
        Totals: new DocumentTotals(835.00m, 0, 167.00m, 1002.00m),
        PoReference: "PO-ACM-221",
        Notes: new List<string> { "Prices exclude delivery.", "Goods remain property of seller until paid in full." },
        Terms: "Net 30");

    public static IReadOnlyList<LabelItem> SampleProductLabels() => new List<LabelItem>
    {
        new("Organic Bananas", "6000123456789", BarcodeSymbology.EAN13, "BAN-001", "$1.20", "Per kg"),
        new("Whole Milk 1L", "6000987654321", BarcodeSymbology.EAN13, "MLK-200", "$0.95"),
        new("Sourdough Loaf", "ABC-12345", BarcodeSymbology.Code128, "BRD-010", "$3.50"),
        new("Sparkling Water", "WTR-330", BarcodeSymbology.Code39, Price: "$0.75")
    };

    public static IReadOnlyList<LabelItem> SampleBarcodeLabels() => new List<LabelItem>
    {
        new("Code128 Demo", "RMS-0001-XYZ", BarcodeSymbology.Code128),
        new("Code39 Demo", "RMS0002", BarcodeSymbology.Code39),
        new("EAN13 Demo", "6000123456789", BarcodeSymbology.EAN13),
        new("QR Demo", "https://rms.local/product/42", BarcodeSymbology.QRCode)
    };
}
