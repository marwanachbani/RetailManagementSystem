using RMS.Modules.Printing.Domain;

namespace RMS.Modules.Printing.Domain;

/// <summary>Describes an installed printer discovered on the machine.</summary>
public sealed record PrinterDescriptor(
    string Name,
    PrinterKind Kind,
    bool IsDefault,
    PrinterStatus Status,
    decimal PaperWidthMm = 80,
    string? Location = null,
    string? Comment = null,
    bool IsShared = false)
{
    public bool IsThermal => Kind == PrinterKind.ThermalPos;
}
