using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Domain.Models;

public sealed record StockAdjustmentReportDocument(
    string ReportTitle,
    DateTime GeneratedAt,
    string? Warehouse = null,
    string? AdjustedBy = null,
    string? Reason = null,
    IReadOnlyList<StockAdjustmentLine>? Lines = null,
    IReadOnlyList<string>? Notes = null)
{
    public decimal TotalAdjustedQuantity => Lines?.Sum(l => l.AdjustedQuantity) ?? 0;
    public decimal TotalAdjustedValue => Lines?.Sum(l => l.AdjustedValue) ?? 0;
}

public sealed record StockAdjustmentLine(
    string ProductName,
    string? Sku,
    decimal PreviousQuantity,
    decimal NewQuantity,
    decimal AdjustedQuantity,
    decimal UnitCost,
    decimal AdjustedValue);

public sealed record InventoryCountSheetDocument(
    string SheetTitle,
    DateTime CountDate,
    string? Warehouse = null,
    string? Location = null,
    IReadOnlyList<InventoryCountLine>? Lines = null);

public sealed record InventoryCountLine(
    string ProductName,
    string? Sku,
    string? Barcode,
    decimal ExpectedQuantity,
    decimal? CountedQuantity = null);

public sealed record StockMovementReportDocument(
    string ReportTitle,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    string? Warehouse = null,
    IReadOnlyList<StockMovementLine>? Movements = null);

public sealed record StockMovementLine(
    DateTime Date,
    string ProductName,
    string? Sku,
    string MovementType,
    decimal InQuantity,
    decimal OutQuantity,
    decimal Balance,
    string? Reference = null);
