using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Domain.Models;

public sealed record PurchaseOrderDocument(
    string PoNumber,
    DateTime OrderDate,
    DocumentParty? Supplier = null,
    string? ShipTo = null,
    IReadOnlyList<DocumentLineItem>? Items = null,
    DocumentTotals? Totals = null,
    DateTime? ExpectedDelivery = null,
    string? PaymentTerms = null,
    IReadOnlyList<string>? Notes = null);

public sealed record GoodsReceivedNoteDocument(
    string GrnNumber,
    string? PoReference,
    DateTime ReceivedDate,
    DocumentParty? Supplier = null,
    IReadOnlyList<DocumentLineItem>? Items = null,
    DocumentTotals? Totals = null,
    IReadOnlyList<string>? Notes = null);

public sealed record SupplierInvoiceDocument(
    string InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate = null,
    string? PoReference = null,
    DocumentParty? Supplier = null,
    IReadOnlyList<DocumentLineItem>? Items = null,
    DocumentTotals? Totals = null,
    string? PaymentTerms = null,
    IReadOnlyList<string>? Notes = null);
