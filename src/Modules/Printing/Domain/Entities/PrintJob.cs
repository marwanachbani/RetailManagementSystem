using RMS.Modules.Printing.Domain;

namespace RMS.Modules.Printing.Domain.Entities;

/// <summary>An auditable record of a document sent to a printer.</summary>
public sealed class PrintJob
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string DocumentType { get; init; } = nameof(global::RMS.Modules.Printing.Domain.DocumentType.Receipt);
    public string? DocumentNumber { get; init; }
    public string PrinterName { get; init; } = string.Empty;
    public PrintJobStatus Status { get; set; } = PrintJobStatus.Queued;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? OutputPath { get; set; }
    public string? ErrorMessage { get; set; }
    public int Copies { get; init; } = 1;

    public static PrintJob Create(
        DocumentType type,
        string? documentNumber,
        string printerName,
        int copies = 1,
        string? outputPath = null) =>
        new()
        {
            DocumentType = type.ToString(),
            DocumentNumber = documentNumber,
            PrinterName = printerName,
            Copies = copies,
            OutputPath = outputPath
        };

    public void MarkCompleted(string? outputPath = null)
    {
        Status = PrintJobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        if (outputPath is not null) OutputPath = outputPath;
    }

    public void MarkFailed(string error)
    {
        Status = PrintJobStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = error;
    }
}
