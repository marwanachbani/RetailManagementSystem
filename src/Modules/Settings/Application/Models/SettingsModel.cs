namespace RMS.Modules.Settings.Application.Models;

/// <summary>
/// Strongly typed, UI-ready projection of every setting grouped by section.
/// Built by the application layer from the raw key/value store.
/// </summary>
public sealed class SettingsModel
{
    public GeneralSettingsModel General { get; set; } = new();
    public StoreSettingsModel Store { get; set; } = new();
    public ReceiptSettingsModel Receipt { get; set; } = new();
    public SalesSettingsModel Sales { get; set; } = new();
    public InventorySettingsModel Inventory { get; set; } = new();
    public PurchasingSettingsModel Purchasing { get; set; } = new();
    public ReportSettingsModel Report { get; set; } = new();
    public BackupSettingsModel Backup { get; set; } = new();
    public ApplicationSettingsModel Application { get; set; } = new();
    public PrinterSettingsModel Printer { get; set; } = new();
    public List<FolderSettingModel> Storage { get; set; } = new();
}

public sealed class GeneralSettingsModel
{
    public string StoreName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string DateFormat { get; set; } = string.Empty;
    public string TimeFormat { get; set; } = string.Empty;
    public string NumberFormat { get; set; } = string.Empty;
}

public sealed class StoreSettingsModel
{
    public string BusinessLogo { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
}

public sealed class ReceiptSettingsModel
{
    public string Header { get; set; } = string.Empty;
    public string Footer { get; set; } = string.Empty;
    public string StoreLogo { get; set; } = string.Empty;
    public bool ShowTaxNumber { get; set; }
    public bool ShowCashier { get; set; }
    public bool ShowBarcode { get; set; }
    public bool ShowQrCode { get; set; }
    public int PaperWidth { get; set; }
    public bool AutomaticPrinting { get; set; }
}

public sealed class SalesSettingsModel
{
    public decimal DefaultTaxRate { get; set; }
    public decimal DefaultDiscount { get; set; }
    public decimal MaximumDiscount { get; set; }
    public bool AllowManualPriceChange { get; set; }
    public bool RequireManagerApproval { get; set; }
    public string DefaultPaymentMethod { get; set; } = string.Empty;
    public bool AutoCompleteSale { get; set; }
    public bool ReceiptPreview { get; set; }
}

public sealed class InventorySettingsModel
{
    public int DefaultLowStockThreshold { get; set; }
    public bool AllowNegativeStock { get; set; }
    public bool AutomaticStockAlerts { get; set; }
    public bool RequireConfirmationForAdjustments { get; set; }
    public string DefaultWarehouse { get; set; } = string.Empty;
    public string DefaultStockAdjustmentReason { get; set; } = string.Empty;
}

public sealed class PurchasingSettingsModel
{
    public string DefaultSupplier { get; set; } = string.Empty;
    public string PurchaseNumberPrefix { get; set; } = string.Empty;
    public bool AutomaticGoodsReceipt { get; set; }
    public string DefaultPaymentTerms { get; set; } = string.Empty;
}

public sealed class ReportSettingsModel
{
    public string DefaultReportFolder { get; set; } = string.Empty;
    public string FileNamePattern { get; set; } = string.Empty;
    public string PdfQuality { get; set; } = string.Empty;
    public string CsvDelimiter { get; set; } = string.Empty;
    public string ExcelExportFormat { get; set; } = string.Empty;
    public string PrintOrientation { get; set; } = string.Empty;
    public bool IncludeCompanyLogo { get; set; }
    public bool IncludeReportFooter { get; set; }
    public bool AutomaticNumbering { get; set; }
}

public sealed class BackupSettingsModel
{
    public bool AutomaticBackup { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public int MaximumCount { get; set; }
    public bool Compress { get; set; }
    public bool VerifyIntegrity { get; set; }
}

public sealed class ApplicationSettingsModel
{
    public string Theme { get; set; } = string.Empty;
    public string StartupPage { get; set; } = string.Empty;
    public bool RememberLastUser { get; set; }
    public bool AutoSave { get; set; }
    public int SessionTimeout { get; set; }
}

public sealed class PrinterSettingsModel
{
    public string DefaultPrinter { get; set; } = string.Empty;
    public string ReceiptPrinter { get; set; } = string.Empty;
    public string InvoicePrinter { get; set; } = string.Empty;
    public string LabelPrinter { get; set; } = string.Empty;
    public string ReportPrinter { get; set; } = string.Empty;
    public bool AutoPrint { get; set; }
    public int Copies { get; set; } = 1;
    public int PaperWidth { get; set; } = 80;
    public string Orientation { get; set; } = "Portrait";
    public int MarginMm { get; set; } = 10;
    public bool CutPaper { get; set; } = true;
    public bool OpenDrawer { get; set; }
}

/// <summary>
/// A single configurable folder path (File Storage section). Exposes both the
/// currently stored/resolved path and the default path so the UI can offer a
/// "Restore Default" action.
/// </summary>
public sealed class FolderSettingModel
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string DefaultPath { get; set; } = string.Empty;
}
