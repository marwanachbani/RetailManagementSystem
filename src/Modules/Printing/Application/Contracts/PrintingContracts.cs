using RMS.BuildingBlocks.Results;
using RMS.Modules.Printing.Application.Models;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Entities;
using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Application.Contracts;

/// <summary>High-level facade used by the WPF shell and business modules. Never exposes raw printer APIs.</summary>
public interface IPrintingService
{
    Task<IReadOnlyList<PrinterDescriptor>> GetPrintersAsync(CancellationToken cancellationToken = default);
    Task<PrinterDescriptor?> GetDefaultPrinterAsync(CancellationToken cancellationToken = default);
    Task<Result<PrinterStatus>> GetPrinterStatusAsync(string printerName, CancellationToken cancellationToken = default);

    /// <summary>Renders a document to PDF bytes (for preview / export).</summary>
    Task<Result<byte[]>> RenderAsync(DocumentType type, object model, CancellationToken cancellationToken = default);

    /// <summary>Renders and prints a document. Returns the path of the saved PDF (for preview/export).</summary>
    Task<Result<string>> PrintAsync(DocumentType type, object model, string? printerName = null, int copies = 1, CancellationToken cancellationToken = default);

    Task<Result<string>> PrintReceiptAsync(ReceiptDocument receipt, string? printerName = null, CancellationToken cancellationToken = default);
    Task<Result<string>> PrintLabelsAsync(IEnumerable<LabelItem> labels, DocumentType labelType, string? printerName = null, CancellationToken cancellationToken = default);

    Task<Result<string>> PrintTestReceiptAsync(string? printerName = null, CancellationToken cancellationToken = default);
    Task<Result<string>> PrintTestInvoiceAsync(string? printerName = null, CancellationToken cancellationToken = default);
    Task<Result<string>> PrintTestLabelAsync(string? printerName = null, CancellationToken cancellationToken = default);
    Task<Result<string>> PrintTestBarcodeAsync(string? printerName = null, CancellationToken cancellationToken = default);
}

public interface IPrinterService
{
    IReadOnlyList<PrinterDescriptor> DiscoverPrinters();
    PrinterDescriptor? GetDefaultPrinter();
    Result<PrinterStatus> GetStatus(string printerName);
    Task<Result> PrintPdfAsync(string printerName, byte[] pdf, PrintOptions options, CancellationToken cancellationToken = default);
    Task<Result> PrintRawAsync(string printerName, byte[] rawBytes, CancellationToken cancellationToken = default);
}

public interface IDocumentRenderingService
{
    byte[] Render(DocumentType type, object model, BrandingInfo branding, PrintOptions options);
}

public interface IReceiptPrinter
{
    Task<Result> PrintReceiptAsync(ReceiptDocument receipt, BrandingInfo branding, string printerName, CancellationToken cancellationToken = default);
}

public interface ILabelPrinter
{
    Task<Result> PrintLabelsAsync(IEnumerable<LabelItem> labels, DocumentType labelType, BrandingInfo branding, string printerName, CancellationToken cancellationToken = default);
}

public interface IPrinterDiscovery
{
    IReadOnlyList<PrinterDescriptor> DiscoverPrinters();
    string? GetDefaultPrinterName();
}

public interface IPrintSettingsProvider
{
    Task<PrintSettings> GetAsync(CancellationToken cancellationToken = default);
}

public interface IBarcodeGenerator
{
    byte[] Generate(string content, BarcodeSymbology symbology, int width, int height, bool pureBarcode = true);
    byte[] GenerateQr(string content, int size);
}

public interface IPrintJobRepository
{
    Task AddAsync(PrintJob job, CancellationToken cancellationToken = default);
    Task UpdateAsync(PrintJob job, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrintJob>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default);
}
