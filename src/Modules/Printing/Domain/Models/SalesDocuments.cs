using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Domain.Models;

public sealed record ReceiptDocument(
    string ReceiptNumber,
    DateTime SaleDate,
    string CashierName,
    IReadOnlyList<DocumentLineItem> Items,
    DocumentTotals Totals,
    string? CustomerName = null,
    string? BarcodeData = null,
    string? QrData = null,
    string ThankYouMessage = "Thank you for your purchase!",
    string? FooterText = null);

public sealed record InvoiceDocument(
    string InvoiceNumber,
    DateTime IssueDate,
    DateTime? DueDate = null,
    DocumentParty? Seller = null,
    DocumentParty? Customer = null,
    IReadOnlyList<DocumentLineItem>? Items = null,
    DocumentTotals? Totals = null,
    string? PoReference = null,
    IReadOnlyList<string>? Notes = null,
    string? Terms = null);

public sealed record RefundReceiptDocument(
    string RefundNumber,
    string OriginalReceiptNumber,
    DateTime Date,
    string CashierName,
    string? CustomerName = null,
    IReadOnlyList<DocumentLineItem>? Items = null,
    DocumentTotals? Totals = null,
    string? Reason = null,
    string? BarcodeData = null,
    string? QrData = null);

public sealed record QuoteDocument(
    string QuoteNumber,
    DateTime IssueDate,
    DateTime ValidUntil,
    DocumentParty? Seller = null,
    DocumentParty? Customer = null,
    IReadOnlyList<DocumentLineItem>? Items = null,
    DocumentTotals? Totals = null,
    IReadOnlyList<string>? Notes = null);

public sealed record DeliveryNoteDocument(
    string DeliveryNumber,
    string? OrderReference,
    DateTime Date,
    DocumentParty? Customer = null,
    string? DeliveryAddress = null,
    string? Carrier = null,
    IReadOnlyList<DocumentLineItem>? Items = null,
    IReadOnlyList<string>? Notes = null);
