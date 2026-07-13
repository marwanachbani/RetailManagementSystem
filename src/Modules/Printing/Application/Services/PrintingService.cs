using RMS.BuildingBlocks.Results;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Application.Models;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Entities;
using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Application.Services;

/// <summary>
/// Orchestrates document rendering and printing. This is the single entry point
/// business modules and the WPF shell use; raw printer APIs live behind the
/// infrastructure adapters and are never exposed here.
/// </summary>
public sealed class PrintingService : IPrintingService
{
    private static readonly string PrintsFolder =
        Path.Combine(Path.GetTempPath(), "RetailManagementSystem", "Prints");

    private readonly IDocumentRenderingService _renderer;
    private readonly IPrinterService _printer;
    private readonly IReceiptPrinter _receiptPrinter;
    private readonly ILabelPrinter _labelPrinter;
    private readonly IPrintSettingsProvider _settings;
    private readonly IPrintJobRepository _jobs;

    public PrintingService(
        IDocumentRenderingService renderer,
        IPrinterService printer,
        IReceiptPrinter receiptPrinter,
        ILabelPrinter labelPrinter,
        IPrintSettingsProvider settings,
        IPrintJobRepository jobs)
    {
        _renderer = renderer;
        _printer = printer;
        _receiptPrinter = receiptPrinter;
        _labelPrinter = labelPrinter;
        _settings = settings;
        _jobs = jobs;
    }

    public Task<IReadOnlyList<PrinterDescriptor>> GetPrintersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_printer.DiscoverPrinters());

    public Task<PrinterDescriptor?> GetDefaultPrinterAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_printer.GetDefaultPrinter());

    public Task<Result<PrinterStatus>> GetPrinterStatusAsync(string printerName, CancellationToken cancellationToken = default) =>
        Task.FromResult(_printer.GetStatus(printerName));

    public async Task<Result<byte[]>> RenderAsync(DocumentType type, object model, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _settings.GetAsync(cancellationToken);
            var pdf = _renderer.Render(type, model, settings.Branding, BuildOptions(settings, type, settings.Copies, model));
            return Result.Success(pdf);
        }
        catch (Exception ex)
        {
            return Result.Failure<byte[]>($"Could not render the {type} document: {ex.Message}", "RENDER_FAILED");
        }
    }

    public async Task<Result<string>> PrintAsync(
        DocumentType type, object model, string? printerName = null, int copies = 1, CancellationToken cancellationToken = default)
    {
        var settings = await _settings.GetAsync(cancellationToken);
        var options = BuildOptions(settings, type, copies, model);

        byte[] pdf;
        try
        {
            pdf = _renderer.Render(type, model, settings.Branding, options);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Could not render the {type} document: {ex.Message}", "RENDER_FAILED");
        }

        var target = string.IsNullOrWhiteSpace(printerName) ? settings.ResolvePrinterFor(type) : printerName!;
        var number = DocumentNumber(type, model);
        var outputPath = SavePdf(pdf, type, number);
        var job = PrintJob.Create(type, number, target, options.Copies, outputPath);
        await _jobs.AddAsync(job, cancellationToken);

        var result = await DispatchAsync(type, model, settings, target, pdf, options, cancellationToken);
        if (result.IsSuccess) job.MarkCompleted(outputPath);
        else job.MarkFailed(result.Error ?? "Print failed");
        await _jobs.UpdateAsync(job, cancellationToken);

        return result.IsSuccess
            ? Result.Success(outputPath)
            : Result.Failure<string>(result.Error ?? "Print failed", result.ErrorCode);
    }

    public Task<Result<string>> PrintReceiptAsync(ReceiptDocument receipt, string? printerName = null, CancellationToken cancellationToken = default) =>
        PrintAsync(DocumentType.Receipt, receipt, printerName, cancellationToken: cancellationToken);

    public Task<Result<string>> PrintLabelsAsync(IEnumerable<LabelItem> labels, DocumentType labelType, string? printerName = null, CancellationToken cancellationToken = default)
    {
        var list = labels.ToList();
        object doc = labelType switch
        {
            DocumentType.ProductLabel => new ProductLabelDocument(list),
            DocumentType.ShelfLabel => new ShelfLabelDocument(list),
            _ => new BarcodeLabelDocument(list)
        };
        return PrintAsync(labelType, doc, printerName, cancellationToken: cancellationToken);
    }

    public Task<Result<string>> PrintTestReceiptAsync(string? printerName = null, CancellationToken cancellationToken = default) =>
        PrintReceiptAsync(SampleData.SampleReceipt(), printerName, cancellationToken);

    public Task<Result<string>> PrintTestInvoiceAsync(string? printerName = null, CancellationToken cancellationToken = default) =>
        PrintAsync(DocumentType.Invoice, SampleData.SampleInvoice(), printerName, cancellationToken: cancellationToken);

    public Task<Result<string>> PrintTestLabelAsync(string? printerName = null, CancellationToken cancellationToken = default) =>
        PrintLabelsAsync(SampleData.SampleProductLabels(), DocumentType.ProductLabel, printerName, cancellationToken);

    public Task<Result<string>> PrintTestBarcodeAsync(string? printerName = null, CancellationToken cancellationToken = default) =>
        PrintLabelsAsync(SampleData.SampleBarcodeLabels(), DocumentType.BarcodeLabel, printerName, cancellationToken);

    // ------------------------------------------------------------------

    private async Task<Result> DispatchAsync(
        DocumentType type, object model, PrintSettings settings, string target, byte[] pdf, PrintOptions options, CancellationToken ct)
    {
        var descriptor = _printer.DiscoverPrinters().FirstOrDefault(p => p.Name == target);

        if (descriptor is { IsThermal: true })
        {
            if (type == DocumentType.Receipt && model is ReceiptDocument receipt)
                return await _receiptPrinter.PrintReceiptAsync(receipt, settings.Branding, target, ct);

            if (type is DocumentType.BarcodeLabel or DocumentType.ProductLabel or DocumentType.ShelfLabel)
            {
                var items = ExtractLabels(model);
                if (items is not null)
                    return await _labelPrinter.PrintLabelsAsync(items, type, settings.Branding, target, ct);
            }
        }

        return await _printer.PrintPdfAsync(target, pdf, options, ct);
    }

    private static List<LabelItem>? ExtractLabels(object model) => model switch
    {
        BarcodeLabelDocument b => b.Items.ToList(),
        ProductLabelDocument p => p.Items.ToList(),
        ShelfLabelDocument s => s.Items.ToList(),
        _ => null
    };

    private static PrintOptions BuildOptions(PrintSettings s, DocumentType type, int copies, object? model = null)
    {
        var orientation = model is ReportDocument r
            ? r.Orientation
            : s.Orientation;
        var paper = type is DocumentType.Receipt or DocumentType.RefundReceipt ? PaperSize.Custom : PaperSize.A4;
        var effectiveCopies = copies > 0 ? copies : s.Copies;
        return new PrintOptions(effectiveCopies, paper, orientation, MarginMm: s.MarginMm, PaperWidthMm: s.PaperWidthMm);
    }

    private static string? DocumentNumber(DocumentType type, object model) => type switch
    {
        DocumentType.Receipt => model is ReceiptDocument r ? r.ReceiptNumber : null,
        DocumentType.RefundReceipt => model is RefundReceiptDocument rf ? rf.RefundNumber : null,
        DocumentType.Invoice => model is InvoiceDocument i ? i.InvoiceNumber : null,
        DocumentType.Quote => model is QuoteDocument q ? q.QuoteNumber : null,
        DocumentType.DeliveryNote => model is DeliveryNoteDocument d ? d.DeliveryNumber : null,
        DocumentType.PurchaseOrder => model is PurchaseOrderDocument p ? p.PoNumber : null,
        DocumentType.GoodsReceivedNote => model is GoodsReceivedNoteDocument g ? g.GrnNumber : null,
        DocumentType.SupplierInvoice => model is SupplierInvoiceDocument si ? si.InvoiceNumber : null,
        DocumentType.CustomerStatement => model is CustomerStatementDocument cs ? cs.StatementNumber : null,
        DocumentType.SupplierStatement => model is SupplierStatementDocument ss ? ss.StatementNumber : null,
        _ => null
    };

    private static string SavePdf(byte[] pdf, DocumentType type, string? number)
    {
        Directory.CreateDirectory(PrintsFolder);
        var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var raw = string.IsNullOrWhiteSpace(number) ? type.ToString() : number!;
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(raw.Select(c => Array.IndexOf(invalid, c) >= 0 ? '_' : c).ToArray());
        var path = Path.Combine(PrintsFolder, $"{type}_{safe}_{stamp}.pdf");
        File.WriteAllBytes(path, pdf);
        return path;
    }
}
