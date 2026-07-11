using RMS.Modules.Settings.Domain.Entities;

namespace RMS.Modules.Settings.Domain;

/// <summary>
/// Single source of truth for the Settings module: the list of categories and the
/// default value of every setting. The infrastructure migration seeds from this
/// catalog, the application layer reads/writes through it, and the WPF UI renders
/// editors based on it.
///
/// Folder-type settings (the File Storage section) are stored as RELATIVE sub-paths
/// by default. The application resolves them against a configurable base directory
/// at read time so that the stored value is always absolute on disk.
/// </summary>
public static class SettingCatalog
{
    // ------------------------------------------------------------------
    // Category names
    // ------------------------------------------------------------------
    public const string CategoryGeneral = "General";
    public const string CategoryStore = "Store";
    public const string CategoryReceipt = "Receipt";
    public const string CategorySales = "Sales";
    public const string CategoryInventory = "Inventory";
    public const string CategoryPurchasing = "Purchasing";
    public const string CategoryReport = "Report";
    public const string CategoryStorage = "Storage";
    public const string CategoryBackup = "Backup";
    public const string CategoryApplication = "Application";

    // ------------------------------------------------------------------
    // Setting keys
    // ------------------------------------------------------------------
    public static class Keys
    {
        // General
        public const string GeneralStoreName = "General.StoreName";
        public const string GeneralPhoneNumber = "General.PhoneNumber";
        public const string GeneralEmail = "General.Email";
        public const string GeneralWebsite = "General.Website";
        public const string GeneralTaxNumber = "General.TaxNumber";
        public const string GeneralCurrency = "General.Currency";
        public const string GeneralTimeZone = "General.TimeZone";
        public const string GeneralLanguage = "General.Language";
        public const string GeneralDateFormat = "General.DateFormat";
        public const string GeneralTimeFormat = "General.TimeFormat";
        public const string GeneralNumberFormat = "General.NumberFormat";

        // Store
        public const string StoreBusinessLogo = "Store.BusinessLogo";
        public const string StoreCompanyAddress = "Store.CompanyAddress";

        // Receipt
        public const string ReceiptHeader = "Receipt.Header";
        public const string ReceiptFooter = "Receipt.Footer";
        public const string ReceiptStoreLogo = "Receipt.StoreLogo";
        public const string ReceiptShowTaxNumber = "Receipt.ShowTaxNumber";
        public const string ReceiptShowCashier = "Receipt.ShowCashier";
        public const string ReceiptShowBarcode = "Receipt.ShowBarcode";
        public const string ReceiptShowQrCode = "Receipt.ShowQrCode";
        public const string ReceiptPaperWidth = "Receipt.PaperWidth";
        public const string ReceiptAutomaticPrinting = "Receipt.AutomaticPrinting";

        // Sales
        public const string SalesDefaultTaxRate = "Sales.DefaultTaxRate";
        public const string SalesDefaultDiscount = "Sales.DefaultDiscount";
        public const string SalesMaximumDiscount = "Sales.MaximumDiscount";
        public const string SalesAllowManualPriceChange = "Sales.AllowManualPriceChange";
        public const string SalesRequireManagerApproval = "Sales.RequireManagerApproval";
        public const string SalesDefaultPaymentMethod = "Sales.DefaultPaymentMethod";
        public const string SalesAutoCompleteSale = "Sales.AutoCompleteSale";
        public const string SalesReceiptPreview = "Sales.ReceiptPreview";

        // Inventory
        public const string InventoryDefaultLowStockThreshold = "Inventory.DefaultLowStockThreshold";
        public const string InventoryAllowNegativeStock = "Inventory.AllowNegativeStock";
        public const string InventoryAutomaticStockAlerts = "Inventory.AutomaticStockAlerts";
        public const string InventoryRequireConfirmationForAdjustments = "Inventory.RequireConfirmationForAdjustments";
        public const string InventoryDefaultWarehouse = "Inventory.DefaultWarehouse";
        public const string InventoryDefaultStockAdjustmentReason = "Inventory.DefaultStockAdjustmentReason";

        // Purchasing
        public const string PurchasingDefaultSupplier = "Purchasing.DefaultSupplier";
        public const string PurchasingPurchaseNumberPrefix = "Purchasing.PurchaseNumberPrefix";
        public const string PurchasingAutomaticGoodsReceipt = "Purchasing.AutomaticGoodsReceipt";
        public const string PurchasingDefaultPaymentTerms = "Purchasing.DefaultPaymentTerms";

        // Report
        public const string ReportDefaultReportFolder = "Report.DefaultReportFolder";
        public const string ReportFileNamePattern = "Report.FileNamePattern";
        public const string ReportPdfQuality = "Report.PdfQuality";
        public const string ReportCsvDelimiter = "Report.CsvDelimiter";
        public const string ReportExcelExportFormat = "Report.ExcelExportFormat";
        public const string ReportPrintOrientation = "Report.PrintOrientation";
        public const string ReportIncludeCompanyLogo = "Report.IncludeCompanyLogo";
        public const string ReportIncludeReportFooter = "Report.IncludeReportFooter";
        public const string ReportAutomaticNumbering = "Report.AutomaticNumbering";

        // Storage (relative sub-paths by default)
        public const string StorageReportsFolder = "Storage.ReportsFolder";
        public const string StorageReceiptsFolder = "Storage.ReceiptsFolder";
        public const string StorageInvoiceFolder = "Storage.InvoiceFolder";
        public const string StorageExportFolder = "Storage.ExportFolder";
        public const string StorageImportFolder = "Storage.ImportFolder";
        public const string StorageBackupFolder = "Storage.BackupFolder";
        public const string StorageDatabaseFolder = "Storage.DatabaseFolder";
        public const string StorageAttachmentsFolder = "Storage.AttachmentsFolder";
        public const string StorageImagesFolder = "Storage.ImagesFolder";
        public const string StorageLogsFolder = "Storage.LogsFolder";
        public const string StorageTempFolder = "Storage.TempFolder";

        // Backup
        public const string BackupAutomaticBackup = "Backup.AutomaticBackup";
        public const string BackupFrequency = "Backup.Frequency";
        public const string BackupTime = "Backup.Time";
        public const string BackupMaximumCount = "Backup.MaximumCount";
        public const string BackupCompress = "Backup.Compress";
        public const string BackupVerifyIntegrity = "Backup.VerifyIntegrity";

        // Application
        public const string ApplicationTheme = "Application.Theme";
        public const string ApplicationStartupPage = "Application.StartupPage";
        public const string ApplicationRememberLastUser = "Application.RememberLastUser";
        public const string ApplicationAutoSave = "Application.AutoSave";
        public const string ApplicationSessionTimeout = "Application.SessionTimeout";
    }

    public sealed record SettingDefinition(
        string Key,
        string Category,
        string DefaultValue,
        SettingDataType DataType,
        string DisplayName,
        bool IsFolder = false,
        string? FolderSubPath = null,
        string? Description = null,
        string[]? Options = null);

    public static IReadOnlyList<SettingCategory> Categories { get; } = new List<SettingCategory>
    {
        new(1, CategoryGeneral, "General", 1),
        new(2, CategoryStore, "Store", 2),
        new(3, CategoryReceipt, "Receipts", 3),
        new(4, CategorySales, "Sales", 4),
        new(5, CategoryInventory, "Inventory", 5),
        new(6, CategoryPurchasing, "Purchasing", 6),
        new(7, CategoryReport, "Reports", 7),
        new(8, CategoryStorage, "File Storage", 8),
        new(9, CategoryBackup, "Backups", 9),
        new(10, CategoryApplication, "Appearance", 10)
    };

    public static IReadOnlyList<SettingDefinition> Defaults { get; } = BuildDefaults();

    private static List<SettingDefinition> BuildDefaults()
    {
        var d = new List<SettingDefinition>();

        // General
        d.Add(new(Keys.GeneralStoreName, CategoryGeneral, "My Retail Store", SettingDataType.String, "Store Name", Description: "Name of the business shown throughout the application."));
        d.Add(new(Keys.GeneralPhoneNumber, CategoryGeneral, "", SettingDataType.String, "Phone Number"));
        d.Add(new(Keys.GeneralEmail, CategoryGeneral, "", SettingDataType.String, "Email", Description: "Contact email for the business."));
        d.Add(new(Keys.GeneralWebsite, CategoryGeneral, "", SettingDataType.String, "Website"));
        d.Add(new(Keys.GeneralTaxNumber, CategoryGeneral, "", SettingDataType.String, "Tax Number"));
        d.Add(new(Keys.GeneralCurrency, CategoryGeneral, "USD", SettingDataType.String, "Currency", Options: new[] { "USD", "EUR", "GBP", "JPY", "AUD", "CAD" }));
        d.Add(new(Keys.GeneralTimeZone, CategoryGeneral, "UTC", SettingDataType.String, "Time Zone"));
        d.Add(new(Keys.GeneralLanguage, CategoryGeneral, "English", SettingDataType.String, "Language", Options: new[] { "English", "Spanish", "French", "German", "Arabic" }));
        d.Add(new(Keys.GeneralDateFormat, CategoryGeneral, "yyyy-MM-dd", SettingDataType.String, "Date Format", Options: new[] { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "dd.MM.yyyy" }));
        d.Add(new(Keys.GeneralTimeFormat, CategoryGeneral, "HH:mm", SettingDataType.String, "Time Format", Options: new[] { "HH:mm", "hh:mm tt" }));
        d.Add(new(Keys.GeneralNumberFormat, CategoryGeneral, "#,##0.00", SettingDataType.String, "Number Format"));

        // Store
        d.Add(new(Keys.StoreBusinessLogo, CategoryStore, "", SettingDataType.String, "Business Logo", Description: "Path to the company logo image."));
        d.Add(new(Keys.StoreCompanyAddress, CategoryStore, "", SettingDataType.String, "Company Address"));

        // Receipt
        d.Add(new(Keys.ReceiptHeader, CategoryReceipt, "Thank you for shopping with us!", SettingDataType.String, "Receipt Header"));
        d.Add(new(Keys.ReceiptFooter, CategoryReceipt, "Returns accepted within 14 days with receipt.", SettingDataType.String, "Receipt Footer"));
        d.Add(new(Keys.ReceiptStoreLogo, CategoryReceipt, "", SettingDataType.String, "Store Logo", Description: "Path to the logo printed on receipts."));
        d.Add(new(Keys.ReceiptShowTaxNumber, CategoryReceipt, "true", SettingDataType.Boolean, "Show Tax Number"));
        d.Add(new(Keys.ReceiptShowCashier, CategoryReceipt, "true", SettingDataType.Boolean, "Show Cashier"));
        d.Add(new(Keys.ReceiptShowBarcode, CategoryReceipt, "true", SettingDataType.Boolean, "Show Barcode"));
        d.Add(new(Keys.ReceiptShowQrCode, CategoryReceipt, "false", SettingDataType.Boolean, "Show QR Code"));
        d.Add(new(Keys.ReceiptPaperWidth, CategoryReceipt, "80", SettingDataType.Integer, "Paper Width (mm)"));
        d.Add(new(Keys.ReceiptAutomaticPrinting, CategoryReceipt, "false", SettingDataType.Boolean, "Automatic Printing"));

        // Sales
        d.Add(new(Keys.SalesDefaultTaxRate, CategorySales, "0", SettingDataType.Decimal, "Default Tax Rate (%)"));
        d.Add(new(Keys.SalesDefaultDiscount, CategorySales, "0", SettingDataType.Decimal, "Default Discount (%)"));
        d.Add(new(Keys.SalesMaximumDiscount, CategorySales, "100", SettingDataType.Decimal, "Maximum Discount (%)"));
        d.Add(new(Keys.SalesAllowManualPriceChange, CategorySales, "false", SettingDataType.Boolean, "Allow Manual Price Change"));
        d.Add(new(Keys.SalesRequireManagerApproval, CategorySales, "false", SettingDataType.Boolean, "Require Manager Approval"));
        d.Add(new(Keys.SalesDefaultPaymentMethod, CategorySales, "Cash", SettingDataType.String, "Default Payment Method", Options: new[] { "Cash", "Card", "Bank Transfer", "Credit" }));
        d.Add(new(Keys.SalesAutoCompleteSale, CategorySales, "false", SettingDataType.Boolean, "Auto Complete Sale"));
        d.Add(new(Keys.SalesReceiptPreview, CategorySales, "true", SettingDataType.Boolean, "Receipt Preview"));

        // Inventory
        d.Add(new(Keys.InventoryDefaultLowStockThreshold, CategoryInventory, "10", SettingDataType.Integer, "Default Low Stock Threshold"));
        d.Add(new(Keys.InventoryAllowNegativeStock, CategoryInventory, "false", SettingDataType.Boolean, "Allow Negative Stock"));
        d.Add(new(Keys.InventoryAutomaticStockAlerts, CategoryInventory, "true", SettingDataType.Boolean, "Automatic Stock Alerts"));
        d.Add(new(Keys.InventoryRequireConfirmationForAdjustments, CategoryInventory, "true", SettingDataType.Boolean, "Require Confirmation For Adjustments"));
        d.Add(new(Keys.InventoryDefaultWarehouse, CategoryInventory, "Main Warehouse", SettingDataType.String, "Default Warehouse"));
        d.Add(new(Keys.InventoryDefaultStockAdjustmentReason, CategoryInventory, "Manual adjustment", SettingDataType.String, "Default Stock Adjustment Reason"));

        // Purchasing
        d.Add(new(Keys.PurchasingDefaultSupplier, CategoryPurchasing, "", SettingDataType.String, "Default Supplier"));
        d.Add(new(Keys.PurchasingPurchaseNumberPrefix, CategoryPurchasing, "PO-", SettingDataType.String, "Purchase Number Prefix"));
        d.Add(new(Keys.PurchasingAutomaticGoodsReceipt, CategoryPurchasing, "false", SettingDataType.Boolean, "Automatic Goods Receipt"));
        d.Add(new(Keys.PurchasingDefaultPaymentTerms, CategoryPurchasing, "Net 30", SettingDataType.String, "Default Payment Terms", Options: new[] { "Net 15", "Net 30", "Net 60", "Due on Receipt" }));

        // Report
        d.Add(new(Keys.ReportDefaultReportFolder, CategoryReport, "Reports", SettingDataType.String, "Default Report Folder"));
        d.Add(new(Keys.ReportFileNamePattern, CategoryReport, "{ReportType}_{yyyyMMdd}", SettingDataType.String, "Report File Name Pattern"));
        d.Add(new(Keys.ReportPdfQuality, CategoryReport, "Standard", SettingDataType.String, "PDF Quality", Options: new[] { "Draft", "Standard", "High" }));
        d.Add(new(Keys.ReportCsvDelimiter, CategoryReport, ",", SettingDataType.String, "CSV Delimiter", Options: new[] { ",", ";", "\\t", "|" }));
        d.Add(new(Keys.ReportExcelExportFormat, CategoryReport, "Xlsx", SettingDataType.String, "Excel Export Format", Options: new[] { "Xlsx", "Xls", "Csv" }));
        d.Add(new(Keys.ReportPrintOrientation, CategoryReport, "Portrait", SettingDataType.String, "Default Print Orientation", Options: new[] { "Portrait", "Landscape" }));
        d.Add(new(Keys.ReportIncludeCompanyLogo, CategoryReport, "true", SettingDataType.Boolean, "Include Company Logo"));
        d.Add(new(Keys.ReportIncludeReportFooter, CategoryReport, "true", SettingDataType.Boolean, "Include Report Footer"));
        d.Add(new(Keys.ReportAutomaticNumbering, CategoryReport, "true", SettingDataType.Boolean, "Automatic Report Numbering"));

        // Storage
        AddFolder(d, Keys.StorageReportsFolder, "Reports Folder", "Reports");
        AddFolder(d, Keys.StorageReceiptsFolder, "Receipts Folder", "Receipts");
        AddFolder(d, Keys.StorageInvoiceFolder, "Invoice Folder", "Invoices");
        AddFolder(d, Keys.StorageExportFolder, "Export Folder", "Exports");
        AddFolder(d, Keys.StorageImportFolder, "Import Folder", "Imports");
        AddFolder(d, Keys.StorageBackupFolder, "Backup Folder", "Backups");
        AddFolder(d, Keys.StorageDatabaseFolder, "Database Folder", "Database");
        AddFolder(d, Keys.StorageAttachmentsFolder, "Attachments Folder", "Attachments");
        AddFolder(d, Keys.StorageImagesFolder, "Images Folder", "Images");
        AddFolder(d, Keys.StorageLogsFolder, "Logs Folder", "Logs");
        AddFolder(d, Keys.StorageTempFolder, "Temporary Files Folder", "Temp");

        // Backup
        d.Add(new(Keys.BackupAutomaticBackup, CategoryBackup, "false", SettingDataType.Boolean, "Automatic Backup"));
        d.Add(new(Keys.BackupFrequency, CategoryBackup, "Daily", SettingDataType.String, "Backup Frequency", Options: new[] { "Daily", "Weekly", "Monthly" }));
        d.Add(new(Keys.BackupTime, CategoryBackup, "23:00", SettingDataType.String, "Backup Time"));
        d.Add(new(Keys.BackupMaximumCount, CategoryBackup, "10", SettingDataType.Integer, "Maximum Backup Count"));
        d.Add(new(Keys.BackupCompress, CategoryBackup, "true", SettingDataType.Boolean, "Compress Backup"));
        d.Add(new(Keys.BackupVerifyIntegrity, CategoryBackup, "true", SettingDataType.Boolean, "Verify Backup Integrity"));

        // Application
        d.Add(new(Keys.ApplicationTheme, CategoryApplication, "Light", SettingDataType.String, "Theme", Options: new[] { "Light", "Dark" }));
        d.Add(new(Keys.ApplicationStartupPage, CategoryApplication, "Dashboard", SettingDataType.String, "Startup Page", Options: new[] { "Dashboard", "Sales", "Inventory", "Products" }));
        d.Add(new(Keys.ApplicationRememberLastUser, CategoryApplication, "true", SettingDataType.Boolean, "Remember Last User"));
        d.Add(new(Keys.ApplicationAutoSave, CategoryApplication, "true", SettingDataType.Boolean, "Auto Save"));
        d.Add(new(Keys.ApplicationSessionTimeout, CategoryApplication, "30", SettingDataType.Integer, "Session Timeout (minutes)"));

        return d;
    }

    private static void AddFolder(List<SettingDefinition> list, string key, string displayName, string subPath)
    {
        list.Add(new(key, CategoryStorage, subPath, SettingDataType.String, displayName, IsFolder: true, FolderSubPath: subPath));
    }

    public static SettingDefinition GetDefinition(string key) =>
        Defaults.First(d => d.Key == key);

    public static IReadOnlyList<SettingDefinition> FolderDefinitions =>
        Defaults.Where(d => d.IsFolder).ToList();
}
