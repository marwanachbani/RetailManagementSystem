using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Domain.Models;

/// <summary>A fully generic, tabular report used by the Reporting module.</summary>
public sealed record ReportDocument(
    string Title,
    DateTime GeneratedAt,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows,
    PrintOrientation Orientation = PrintOrientation.Portrait,
    string? Subtitle = null,
    IReadOnlyDictionary<string, string>? Summary = null,
    string? FooterNote = null,
    string? CurrencyCode = null);
