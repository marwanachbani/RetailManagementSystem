namespace RMS.Modules.Printing.Domain;

/// <summary>Every printable document supported by the retail ERP.</summary>
public enum DocumentType
{
    Receipt,
    Invoice,
    RefundReceipt,
    Quote,
    DeliveryNote,
    PurchaseOrder,
    GoodsReceivedNote,
    SupplierInvoice,
    StockAdjustmentReport,
    InventoryCountSheet,
    StockMovementReport,
    BarcodeLabel,
    ProductLabel,
    ShelfLabel,
    CustomerStatement,
    CustomerPurchaseHistory,
    SupplierStatement,
    SupplierPurchaseHistory,
    Report
}

public enum PrinterKind
{
    Windows,
    ThermalPos
}

public enum PaperSize
{
    Thermal58Mm,
    Thermal80Mm,
    A4,
    A5,
    Letter,
    Custom
}

public enum PrintOrientation
{
    Portrait,
    Landscape
}

public enum BarcodeSymbology
{
    EAN13,
    Code128,
    Code39,
    QRCode
}

public enum PrintJobStatus
{
    Queued,
    Printing,
    Completed,
    Failed,
    Cancelled
}

public enum PrinterStatus
{
    Ready,
    Offline,
    OutOfPaper,
    Error,
    Unknown
}
