using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Application.Models;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Infrastructure.QuestPdf;

/// <summary>
/// Renders every supported document to PDF using reusable templates. Branding is
/// injected from the Settings module; barcodes/QR codes are produced by
/// <see cref="IBarcodeGenerator"/> and embedded as images.
/// </summary>
public sealed class QuestPdfDocumentRenderer : IDocumentRenderingService
{
    private readonly IBarcodeGenerator _barcodes;

    static QuestPdfDocumentRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public QuestPdfDocumentRenderer(IBarcodeGenerator barcodes) => _barcodes = barcodes;

    public byte[] Render(DocumentType type, object model, BrandingInfo branding, PrintOptions options)
    {
        return Document.Create(container => container.Page(page =>
        {
            ConfigurePage(page, type, options);
            page.Header().Element(c => RenderHeader(c, type, branding, model));
            page.Content().Element(c => RenderBody(c, type, model, branding, options));
            page.Footer().Element(c => RenderFooter(c, type, branding, model));
        })).GeneratePdf();
    }

    // ------------------------------------------------------------------

    private static void ConfigurePage(PageDescriptor page, DocumentType type, PrintOptions options)
    {
        if (type is DocumentType.Receipt or DocumentType.RefundReceipt)
            page.Size((float)(options.PaperWidthMm * 2.83464567), 842f);
        else if (options.Orientation == PrintOrientation.Landscape)
            page.Size(PageSizes.A4.Landscape());
        else
            page.Size(PageSizes.A4);

        page.Margin(options.MarginMm, Unit.Millimetre);
        page.DefaultTextStyle(x => x.FontSize(10));
    }

    private static void RenderHeader(IContainer container, DocumentType type, BrandingInfo branding, object model)
    {
        container.Column(col =>
        {
            if (branding.HasLogo)
            {
                try { col.Item().AlignCenter().Height(48).Image(File.ReadAllBytes(branding.LogoPath)); }
                catch { /* ignore missing/invalid logo */ }
            }

            col.Item().AlignCenter().Text(branding.StoreName).Bold().FontSize(16);
            if (!string.IsNullOrWhiteSpace(branding.Address))
                col.Item().AlignCenter().Text(branding.Address).FontSize(9);

            var contact = string.Join("  |  ", new[] { branding.Phone, branding.TaxNumber }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
            if (!string.IsNullOrWhiteSpace(contact))
                col.Item().AlignCenter().Text(contact).FontSize(9);

            col.Item().PaddingTop(6).AlignCenter().Text(DocumentTitle(type, model)).Bold().FontSize(14);

            if (!string.IsNullOrWhiteSpace(branding.ReceiptHeader) &&
                type is DocumentType.Receipt or DocumentType.RefundReceipt)
                col.Item().PaddingTop(2).AlignCenter().Text(branding.ReceiptHeader).FontSize(9);
        });
    }

    private static void RenderFooter(IContainer container, DocumentType type, BrandingInfo branding, object model)
    {
        container.AlignCenter().Text(t =>
        {
            t.DefaultTextStyle(x => x.FontSize(8));
            var note = FooterNote(type, branding, model);
            if (!string.IsNullOrWhiteSpace(note))
                t.Span(note + "    ");

            t.Span("Page ");
            t.CurrentPageNumber();
            t.Span(" of ");
            t.TotalPages();
        });
    }

    private static string DocumentTitle(DocumentType type, object model) => type switch
    {
        DocumentType.Receipt => "SALES RECEIPT",
        DocumentType.RefundReceipt => "REFUND RECEIPT",
        DocumentType.Invoice => "INVOICE",
        DocumentType.Quote => "QUOTE",
        DocumentType.DeliveryNote => "DELIVERY NOTE",
        DocumentType.PurchaseOrder => "PURCHASE ORDER",
        DocumentType.GoodsReceivedNote => "GOODS RECEIVED NOTE",
        DocumentType.SupplierInvoice => "SUPPLIER INVOICE",
        DocumentType.StockAdjustmentReport => (model as StockAdjustmentReportDocument)?.ReportTitle ?? "STOCK ADJUSTMENT REPORT",
        DocumentType.InventoryCountSheet => (model as InventoryCountSheetDocument)?.SheetTitle ?? "INVENTORY COUNT SHEET",
        DocumentType.StockMovementReport => (model as StockMovementReportDocument)?.ReportTitle ?? "STOCK MOVEMENT REPORT",
        DocumentType.BarcodeLabel => "BARCODE LABELS",
        DocumentType.ProductLabel => "PRODUCT LABELS",
        DocumentType.ShelfLabel => "SHELF LABELS",
        DocumentType.CustomerStatement => "CUSTOMER STATEMENT",
        DocumentType.CustomerPurchaseHistory => "CUSTOMER PURCHASE HISTORY",
        DocumentType.SupplierStatement => "SUPPLIER STATEMENT",
        DocumentType.SupplierPurchaseHistory => "SUPPLIER PURCHASE HISTORY",
        _ => "REPORT"
    };

    private static string? FooterNote(DocumentType type, BrandingInfo branding, object model)
    {
        if (type is DocumentType.Receipt && model is ReceiptDocument r)
            return r.FooterText ?? branding.ReceiptFooter;
        if (type is DocumentType.RefundReceipt && model is RefundReceiptDocument)
            return branding.ReceiptFooter;
        if (type is DocumentType.CustomerStatement or DocumentType.SupplierStatement)
            return branding.ReceiptFooter;
        return null;
    }

    // ------------------------------------------------------------------

    private void RenderBody(IContainer container, DocumentType type, object model, BrandingInfo branding, PrintOptions options)
    {
        switch (type)
        {
            case DocumentType.Receipt when model is ReceiptDocument r:
            case DocumentType.RefundReceipt when model is RefundReceiptDocument:
                RenderReceiptBody(container, model, branding);
                break;
            case DocumentType.Invoice when model is InvoiceDocument i: RenderInvoiceBody(container, i, "INVOICE"); break;
            case DocumentType.Quote when model is QuoteDocument q: RenderQuoteBody(container, q); break;
            case DocumentType.DeliveryNote when model is DeliveryNoteDocument d: RenderDeliveryNoteBody(container, d); break;
            case DocumentType.PurchaseOrder when model is PurchaseOrderDocument p: RenderPurchaseOrderBody(container, p); break;
            case DocumentType.GoodsReceivedNote when model is GoodsReceivedNoteDocument g: RenderGrnBody(container, g); break;
            case DocumentType.SupplierInvoice when model is SupplierInvoiceDocument s: RenderSupplierInvoiceBody(container, s); break;
            case DocumentType.StockAdjustmentReport when model is StockAdjustmentReportDocument sa: RenderStockAdjustmentBody(container, sa); break;
            case DocumentType.InventoryCountSheet when model is InventoryCountSheetDocument cs: RenderCountSheetBody(container, cs); break;
            case DocumentType.StockMovementReport when model is StockMovementReportDocument sm: RenderMovementBody(container, sm); break;
            case DocumentType.BarcodeLabel when model is BarcodeLabelDocument b: RenderLabelSheet(container, b.Items, b.Layout, branding); break;
            case DocumentType.ProductLabel when model is ProductLabelDocument p: RenderLabelSheet(container, p.Items, p.Layout, branding); break;
            case DocumentType.ShelfLabel when model is ShelfLabelDocument s: RenderLabelSheet(container, s.Items, s.Layout, branding); break;
            case DocumentType.CustomerStatement when model is CustomerStatementDocument c: RenderCustomerStatement(container, c); break;
            case DocumentType.CustomerPurchaseHistory when model is CustomerPurchaseHistoryDocument c: RenderCustomerHistory(container, c); break;
            case DocumentType.SupplierStatement when model is SupplierStatementDocument s: RenderSupplierStatement(container, s); break;
            case DocumentType.SupplierPurchaseHistory when model is SupplierPurchaseHistoryDocument s: RenderSupplierHistory(container, s); break;
            case DocumentType.Report when model is ReportDocument rep: RenderGenericReport(container, rep); break;
            default: container.Text("Unsupported document type."); break;
        }
    }

    private void RenderReceiptBody(IContainer container, object model, BrandingInfo branding)
    {
        var isRefund = model is RefundReceiptDocument;
        var receipt = model as ReceiptDocument;
        var refund = model as RefundReceiptDocument;

        container.Column(col =>
        {
            col.Spacing(4);

            col.Item().Row(r =>
            {
                r.RelativeItem().Text(isRefund ? $"Refund: {refund!.RefundNumber}" : $"Receipt: {receipt!.ReceiptNumber}").FontSize(10);
                r.RelativeItem().AlignRight().Text((isRefund ? refund.Date : receipt.SaleDate).ToString("g")).FontSize(10);
            });

            if (isRefund)
                col.Item().Text($"Original Receipt: {refund.OriginalReceiptNumber}");
            if (!string.IsNullOrWhiteSpace(isRefund ? refund.CashierName : receipt.CashierName))
                col.Item().Text($"Cashier: {refund?.CashierName ?? receipt!.CashierName}");
            var customer = isRefund ? refund.CustomerName : receipt.CustomerName;
            if (!string.IsNullOrWhiteSpace(customer))
                col.Item().Text($"Customer: {customer}");

            col.Item().LineHorizontal(0.5f);

            RenderItemsTable(col, isRefund ? refund.Items : receipt.Items, true, true);

            if (isRefund ? refund.Totals is not null : receipt.Totals is not null)
            {
                var totals = isRefund ? refund.Totals! : receipt.Totals!;
                col.Item().AlignRight().Column(t =>
                {
                    t.Item().Text($"Subtotal: {totals.SubTotal:F2}");
                    if (totals.DiscountTotal != 0) t.Item().Text($"Discount: -{totals.DiscountTotal:F2}");
                    if (totals.TaxTotal != 0) t.Item().Text($"Tax: {totals.TaxTotal:F2}");
                    t.Item().Text($"TOTAL: {totals.GrandTotal:F2}").Bold().FontSize(13);
                    if (totals.PaidAmount.HasValue) t.Item().Text($"Paid ({totals.PaymentMethod}): {totals.PaidAmount:F2}");
                    if (totals.Change.HasValue) t.Item().Text($"Change: {totals.Change:F2}");
                });
            }

            col.Item().LineHorizontal(0.5f);

            if (!isRefund && !string.IsNullOrWhiteSpace(receipt!.BarcodeData))
                RenderBarcode(col, receipt.BarcodeData!, BarcodeSymbology.Code128, 220, 70);
            if (!isRefund && !string.IsNullOrWhiteSpace(receipt.QrData))
                RenderBarcode(col, receipt.QrData!, BarcodeSymbology.QRCode, 110, 110);
            if (isRefund && !string.IsNullOrWhiteSpace(refund!.BarcodeData))
                RenderBarcode(col, refund.BarcodeData!, BarcodeSymbology.Code128, 220, 70);

            if (!string.IsNullOrWhiteSpace(isRefund ? refund.Reason : null))
                col.Item().Text($"Reason: {refund!.Reason}");

            col.Item().PaddingTop(6).AlignCenter().Text(isRefund ? "Thank you!" : receipt.ThankYouMessage).FontSize(10);
        });
    }

    private static void RenderItemsTable(ColumnDescriptor col, IReadOnlyList<DocumentLineItem>? items, bool showDiscount, bool showTax)
    {
        if (items is null || items.Count == 0) return;

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(4);
                cols.RelativeColumn();
                cols.RelativeColumn();
                cols.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Text("Item").Bold();
                header.Cell().AlignRight().Text("Qty").Bold();
                header.Cell().AlignRight().Text("Unit").Bold();
                header.Cell().AlignRight().Text("Total").Bold();
            });

            foreach (var it in items)
            {
                table.Cell().Text(it.Name + (string.IsNullOrWhiteSpace(it.Description) ? "" : $"\n{it.Description}")).FontSize(9);
                table.Cell().AlignRight().Text(it.Quantity.ToString("F2")).FontSize(9);
                table.Cell().AlignRight().Text(it.UnitPrice.ToString("F2")).FontSize(9);
                table.Cell().AlignRight().Text(it.LineTotal.ToString("F2")).FontSize(9);
            }
        });
    }

    private static void RenderParty(IContainer container, string heading, DocumentParty? party)
    {
        if (party is null) return;
        container.Column(col =>
        {
            col.Item().Text(heading).Bold();
            col.Item().Text(party.Name);
            if (!string.IsNullOrWhiteSpace(party.Address)) col.Item().Text(party.Address);
            if (!string.IsNullOrWhiteSpace(party.Phone)) col.Item().Text($"Tel: {party.Phone}");
            if (!string.IsNullOrWhiteSpace(party.TaxNumber)) col.Item().Text($"Tax No: {party.TaxNumber}");
            if (!string.IsNullOrWhiteSpace(party.AccountNumber)) col.Item().Text($"Acc: {party.AccountNumber}");
        });
    }

    private static void RenderTotals(IContainer container, DocumentTotals totals)
    {
        container.AlignRight().Column(col =>
        {
            col.Item().Text($"Subtotal: {totals.SubTotal:F2}");
            if (totals.DiscountTotal != 0) col.Item().Text($"Discount: -{totals.DiscountTotal:F2}");
            if (totals.TaxTotal != 0) col.Item().Text($"Tax: {totals.TaxTotal:F2}");
            col.Item().Text($"TOTAL: {totals.GrandTotal:F2}").Bold().FontSize(13);
        });
    }

    private static void RenderInvoiceBody(IContainer container, InvoiceDocument inv, string title)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().Text($"{title}: {inv.InvoiceNumber}");
                    c.Item().Text($"Date: {inv.IssueDate:yyyy-MM-dd}");
                    if (inv.DueDate.HasValue) c.Item().Text($"Due: {inv.DueDate:yyyy-MM-dd}");
                    if (!string.IsNullOrWhiteSpace(inv.PoReference)) c.Item().Text($"PO: {inv.PoReference}");
                });
                r.RelativeItem().Element(c => RenderParty(c, "Bill To", inv.Customer));
            });

            col.Item().Element(c => RenderParty(c, "From", inv.Seller));

            if (inv.Items is not null)
                RenderItemsTable(col, inv.Items, true, true);
            if (inv.Totals is not null)
                col.Item().Element(c => RenderTotals(c, inv.Totals));

            if (inv.Notes is { Count: > 0 })
                col.Item().Column(c => { foreach (var n in inv.Notes) c.Item().Text("• " + n).FontSize(9); });
            if (!string.IsNullOrWhiteSpace(inv.Terms))
                col.Item().Text($"Terms: {inv.Terms}").FontSize(9);
        });
    }

    private static void RenderQuoteBody(IContainer container, QuoteDocument q)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().Text($"QUOTE: {q.QuoteNumber}");
                    c.Item().Text($"Date: {q.IssueDate:yyyy-MM-dd}");
                    c.Item().Text($"Valid Until: {q.ValidUntil:yyyy-MM-dd}");
                });
                r.RelativeItem().Element(c => RenderParty(c, "Customer", q.Customer));
            });
            col.Item().Element(c => RenderParty(c, "From", q.Seller));
            if (q.Items is not null) RenderItemsTable(col, q.Items, true, true);
            if (q.Totals is not null) col.Item().Element(c => RenderTotals(c, q.Totals));
            if (q.Notes is { Count: > 0 })
                col.Item().Column(c => { foreach (var n in q.Notes) c.Item().Text("• " + n).FontSize(9); });
        });
    }

    private static void RenderDeliveryNoteBody(IContainer container, DeliveryNoteDocument d)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Delivery: {d.DeliveryNumber}");
                    c.Item().Text($"Date: {d.Date:yyyy-MM-dd}");
                    if (!string.IsNullOrWhiteSpace(d.OrderReference)) c.Item().Text($"Order: {d.OrderReference}");
                    if (!string.IsNullOrWhiteSpace(d.Carrier)) c.Item().Text($"Carrier: {d.Carrier}");
                });
                r.RelativeItem().Element(c => RenderParty(c, "Deliver To", d.Customer));
            });
            if (!string.IsNullOrWhiteSpace(d.DeliveryAddress))
                col.Item().Text($"Address: {d.DeliveryAddress}");
            if (d.Items is not null) RenderItemsTable(col, d.Items, false, false);
            if (d.Notes is { Count: > 0 })
                col.Item().Column(c => { foreach (var n in d.Notes) c.Item().Text("• " + n).FontSize(9); });
        });
    }

    private static void RenderPurchaseOrderBody(IContainer container, PurchaseOrderDocument p)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().Text($"PO: {p.PoNumber}");
                    c.Item().Text($"Date: {p.OrderDate:yyyy-MM-dd}");
                    if (p.ExpectedDelivery.HasValue) c.Item().Text($"Expected: {p.ExpectedDelivery:yyyy-MM-dd}");
                    if (!string.IsNullOrWhiteSpace(p.PaymentTerms)) c.Item().Text($"Terms: {p.PaymentTerms}");
                });
                r.RelativeItem().Element(c => RenderParty(c, "Supplier", p.Supplier));
            });
            if (!string.IsNullOrWhiteSpace(p.ShipTo))
                col.Item().Text($"Ship To: {p.ShipTo}");
            if (p.Items is not null) RenderItemsTable(col, p.Items, true, true);
            if (p.Totals is not null) col.Item().Element(c => RenderTotals(c, p.Totals));
            if (p.Notes is { Count: > 0 })
                col.Item().Column(c => { foreach (var n in p.Notes) c.Item().Text("• " + n).FontSize(9); });
        });
    }

    private static void RenderGrnBody(IContainer container, GoodsReceivedNoteDocument g)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().Text($"GRN: {g.GrnNumber}");
                    c.Item().Text($"Date: {g.ReceivedDate:yyyy-MM-dd}");
                    if (!string.IsNullOrWhiteSpace(g.PoReference)) c.Item().Text($"PO: {g.PoReference}");
                });
                r.RelativeItem().Element(c => RenderParty(c, "Supplier", g.Supplier));
            });
            if (g.Items is not null) RenderItemsTable(col, g.Items, true, true);
            if (g.Totals is not null) col.Item().Element(c => RenderTotals(c, g.Totals));
            if (g.Notes is { Count: > 0 })
                col.Item().Column(c => { foreach (var n in g.Notes) c.Item().Text("• " + n).FontSize(9); });
        });
    }

    private static void RenderSupplierInvoiceBody(IContainer container, SupplierInvoiceDocument s)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Invoice: {s.InvoiceNumber}");
                    c.Item().Text($"Date: {s.InvoiceDate:yyyy-MM-dd}");
                    if (s.DueDate.HasValue) c.Item().Text($"Due: {s.DueDate:yyyy-MM-dd}");
                    if (!string.IsNullOrWhiteSpace(s.PoReference)) c.Item().Text($"PO: {s.PoReference}");
                    if (!string.IsNullOrWhiteSpace(s.PaymentTerms)) c.Item().Text($"Terms: {s.PaymentTerms}");
                });
                r.RelativeItem().Element(c => RenderParty(c, "From (Supplier)", s.Supplier));
            });
            if (s.Items is not null) RenderItemsTable(col, s.Items, true, true);
            if (s.Totals is not null) col.Item().Element(c => RenderTotals(c, s.Totals));
            if (s.Notes is { Count: > 0 })
                col.Item().Column(c => { foreach (var n in s.Notes) c.Item().Text("• " + n).FontSize(9); });
        });
    }

    private static void RenderStockAdjustmentBody(IContainer container, StockAdjustmentReportDocument sa)
    {
        container.Column(col =>
        {
            col.Spacing(4);
            col.Item().Text($"Generated: {sa.GeneratedAt:yyyy-MM-dd HH:mm}").FontSize(9);
            var meta = string.Join("   ", new[] { sa.Warehouse, sa.AdjustedBy, sa.Reason }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (!string.IsNullOrWhiteSpace(meta)) col.Item().Text(meta).FontSize(9);

            if (sa.Lines is not null)
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                        c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Product").Bold(); h.Cell().Text("SKU").Bold();
                        h.Cell().AlignRight().Text("Prev").Bold(); h.Cell().AlignRight().Text("New").Bold();
                        h.Cell().AlignRight().Text("Adj").Bold(); h.Cell().AlignRight().Text("Cost").Bold();
                        h.Cell().AlignRight().Text("Value").Bold();
                    });
                    foreach (var l in sa.Lines)
                    {
                        table.Cell().Text(l.ProductName).FontSize(9);
                        table.Cell().Text(l.Sku ?? "").FontSize(9);
                        table.Cell().AlignRight().Text(l.PreviousQuantity.ToString("F2")).FontSize(9);
                        table.Cell().AlignRight().Text(l.NewQuantity.ToString("F2")).FontSize(9);
                        table.Cell().AlignRight().Text(l.AdjustedQuantity.ToString("F2")).FontSize(9);
                        table.Cell().AlignRight().Text(l.UnitCost.ToString("F2")).FontSize(9);
                        table.Cell().AlignRight().Text(l.AdjustedValue.ToString("F2")).FontSize(9);
                    }
                });

            col.Item().Row(r =>
            {
                r.RelativeItem().Text("");
                r.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text($"Total Adjusted Qty: {sa.TotalAdjustedQuantity:F2}");
                    c.Item().Text($"Total Adjusted Value: {sa.TotalAdjustedValue:F2}").Bold();
                });
            });
            if (sa.Notes is { Count: > 0 })
                col.Item().Column(c => { foreach (var n in sa.Notes) c.Item().Text("• " + n).FontSize(9); });
        });
    }

    private static void RenderCountSheetBody(IContainer container, InventoryCountSheetDocument cs)
    {
        container.Column(col =>
        {
            col.Spacing(4);
            var meta = string.Join("   ", new[] { cs.Warehouse, cs.Location }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (!string.IsNullOrWhiteSpace(meta)) col.Item().Text(meta).FontSize(9);
            col.Item().Text($"Count Date: {cs.CountDate:yyyy-MM-dd}").FontSize(9);

            if (cs.Lines is not null)
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Product").Bold(); h.Cell().Text("SKU").Bold();
                        h.Cell().Text("Barcode").Bold(); h.Cell().AlignRight().Text("Expected").Bold();
                        h.Cell().Text("Counted").Bold();
                    });
                    foreach (var l in cs.Lines)
                    {
                        table.Cell().Text(l.ProductName).FontSize(9);
                        table.Cell().Text(l.Sku ?? "").FontSize(9);
                        table.Cell().Text(l.Barcode ?? "").FontSize(9);
                        table.Cell().AlignRight().Text(l.ExpectedQuantity.ToString("F2")).FontSize(9);
                        table.Cell().Text("");
                    }
                });
        });
    }

    private static void RenderMovementBody(IContainer container, StockMovementReportDocument sm)
    {
        container.Column(col =>
        {
            col.Spacing(4);
            col.Item().Text($"Period: {sm.PeriodFrom:yyyy-MM-dd} to {sm.PeriodTo:yyyy-MM-dd}").FontSize(9);
            if (!string.IsNullOrWhiteSpace(sm.Warehouse)) col.Item().Text(sm.Warehouse).FontSize(9);

            if (sm.Movements is not null)
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); c.RelativeColumn(3); c.RelativeColumn(); c.RelativeColumn();
                        c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Date").Bold(); h.Cell().Text("Product").Bold();
                        h.Cell().Text("Type").Bold(); h.Cell().AlignRight().Text("In").Bold();
                        h.Cell().AlignRight().Text("Out").Bold(); h.Cell().AlignRight().Text("Bal").Bold();
                        h.Cell().Text("Ref").Bold();
                    });
                    foreach (var m in sm.Movements)
                    {
                        table.Cell().Text(m.Date.ToString("yyyy-MM-dd")).FontSize(9);
                        table.Cell().Text(m.ProductName).FontSize(9);
                        table.Cell().Text(m.MovementType).FontSize(9);
                        table.Cell().AlignRight().Text(m.InQuantity.ToString("F2")).FontSize(9);
                        table.Cell().AlignRight().Text(m.OutQuantity.ToString("F2")).FontSize(9);
                        table.Cell().AlignRight().Text(m.Balance.ToString("F2")).FontSize(9);
                        table.Cell().Text(m.Reference ?? "").FontSize(9);
                    }
                });
        });
    }

    private void RenderLabelSheet(IContainer container, IReadOnlyList<LabelItem> items, LabelLayout layout, BrandingInfo branding)
    {
        var cell = (IContainer c, LabelItem item) => RenderLabelCell(c, item, layout, branding);

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                for (var i = 0; i < layout.Columns; i++) c.RelativeColumn();
            });

            foreach (var item in items)
            {
                table.Cell()
                    .Border(0.5f)
                    .Padding(4)
                    .Element(c => cell(c, item));
            }
        });
    }

    private void RenderLabelCell(IContainer container, LabelItem item, LabelLayout layout, BrandingInfo branding)
    {
        container.Column(col =>
        {
            if (layout.ShowName && !string.IsNullOrWhiteSpace(item.Name))
                col.Item().AlignCenter().Text(item.Name).Bold().FontSize(9);

            if (layout.ShowBarcode)
                RenderBarcode(col, item.BarcodeValue, item.Symbology, 180, item.Symbology == BarcodeSymbology.QRCode ? 90 : 60);

            if (layout.ShowPrice && !string.IsNullOrWhiteSpace(item.Price))
                col.Item().AlignCenter().Text(item.Price).Bold().FontSize(12);

            if (layout.ShowSku && !string.IsNullOrWhiteSpace(item.Sku))
                col.Item().AlignCenter().Text(item.Sku).FontSize(7);

            if (!string.IsNullOrWhiteSpace(item.ExtraLine))
                col.Item().AlignCenter().Text(item.ExtraLine).FontSize(7);
        });
    }

    private void RenderBarcode(ColumnDescriptor col, string content, BarcodeSymbology symbology, int width, int height)
    {
        try
        {
            var bytes = _barcodes.Generate(content, symbology, width, height);
            col.Item().AlignCenter().Image(bytes).FitWidth();
        }
        catch
        {
            col.Item().AlignCenter().Text($"[{content}]").FontSize(8);
        }
    }

    private static void RenderCustomerStatement(IContainer container, CustomerStatementDocument s)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            col.Item().Element(c => RenderParty(c, "Customer", s.Customer));
            col.Item().Row(r =>
            {
                r.RelativeItem().Text($"Statement: {s.StatementNumber}");
                r.RelativeItem().AlignRight().Text($"{s.PeriodFrom:yyyy-MM-dd} to {s.PeriodTo:yyyy-MM-dd}");
            });
            col.Item().Text($"Opening Balance: {s.OpeningBalance:F2}");

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(3);
                    c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                });
                table.Header(h =>
                {
                    h.Cell().Text("Date").Bold(); h.Cell().Text("Ref").Bold(); h.Cell().Text("Description").Bold();
                    h.Cell().AlignRight().Text("Debit").Bold(); h.Cell().AlignRight().Text("Credit").Bold();
                    h.Cell().AlignRight().Text("Balance").Bold();
                });
                foreach (var l in s.Lines)
                {
                    table.Cell().Text(l.Date.ToString("yyyy-MM-dd")).FontSize(9);
                    table.Cell().Text(l.Reference).FontSize(9);
                    table.Cell().Text(l.Description).FontSize(9);
                    table.Cell().AlignRight().Text(l.Debit.ToString("F2")).FontSize(9);
                    table.Cell().AlignRight().Text(l.Credit.ToString("F2")).FontSize(9);
                    table.Cell().AlignRight().Text(l.Balance.ToString("F2")).FontSize(9);
                }
            });

            col.Item().AlignRight().Text($"Closing Balance: {s.ClosingBalance:F2}").Bold().FontSize(12);
        });
    }

    private static void RenderCustomerHistory(IContainer container, CustomerPurchaseHistoryDocument h)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            col.Item().Element(c => RenderParty(c, "Customer", h.Customer));
            col.Item().Text($"Period: {h.PeriodFrom:yyyy-MM-dd} to {h.PeriodTo:yyyy-MM-dd}");

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                });
                table.Header(hd =>
                {
                    hd.Cell().Text("Date").Bold(); hd.Cell().Text("Reference").Bold();
                    hd.Cell().AlignRight().Text("Items").Bold(); hd.Cell().AlignRight().Text("Total").Bold();
                });
                foreach (var o in h.Orders)
                {
                    table.Cell().Text(o.Date.ToString("yyyy-MM-dd")).FontSize(9);
                    table.Cell().Text(o.Reference).FontSize(9);
                    table.Cell().AlignRight().Text(o.ItemCount.ToString()).FontSize(9);
                    table.Cell().AlignRight().Text(o.Total.ToString("F2")).FontSize(9);
                }
            });

            col.Item().AlignRight().Column(c =>
            {
                c.Item().Text($"Orders: {h.OrderCount}");
                c.Item().Text($"Total Spent: {h.TotalSpent:F2}").Bold();
                if (h.LastPurchaseDate.HasValue) c.Item().Text($"Last Purchase: {h.LastPurchaseDate:yyyy-MM-dd}");
            });
        });
    }

    private static void RenderSupplierStatement(IContainer container, SupplierStatementDocument s) =>
        RenderCustomerStatement(container, new CustomerStatementDocument(
            s.StatementNumber, s.PeriodFrom, s.PeriodTo, s.Supplier, s.OpeningBalance, s.Lines, s.ClosingBalance, s.CurrencyCode));

    private static void RenderSupplierHistory(IContainer container, SupplierPurchaseHistoryDocument h) =>
        RenderCustomerHistory(container, new CustomerPurchaseHistoryDocument(
            h.Supplier, h.PeriodFrom, h.PeriodTo, h.Orders, h.TotalSpent, h.OrderCount, h.LastPurchaseDate, h.CurrencyCode));

    private static void RenderGenericReport(IContainer container, ReportDocument rep)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            if (!string.IsNullOrWhiteSpace(rep.Subtitle))
                col.Item().AlignCenter().Text(rep.Subtitle).FontSize(11);
            col.Item().AlignCenter().Text($"Generated: {rep.GeneratedAt:yyyy-MM-dd HH:mm}").FontSize(9);

            col.Item().Table(table =>
            {
                var colCount = rep.Columns.Count;
                table.ColumnsDefinition(c =>
                {
                    for (var i = 0; i < colCount; i++) c.RelativeColumn();
                });
                table.Header(h =>
                {
                    for (var i = 0; i < colCount; i++)
                        h.Cell().Text(rep.Columns[i]).Bold().FontSize(9);
                });
                foreach (var row in rep.Rows)
                {
                    for (var i = 0; i < colCount; i++)
                        table.Cell().Text(row.Count > i ? row[i]?.ToString() ?? "" : "").FontSize(9);
                }
            });

            if (rep.Summary is { Count: > 0 })
                col.Item().AlignRight().Column(c =>
                {
                    foreach (var kv in rep.Summary)
                        c.Item().Text($"{kv.Key}: {kv.Value}").FontSize(10);
                });

            if (!string.IsNullOrWhiteSpace(rep.FooterNote))
                col.Item().Text(rep.FooterNote).FontSize(9);
        });
    }
}
