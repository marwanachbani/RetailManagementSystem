using RMS.Modules.Settings.Application.Models;
using RMS.Modules.Settings.Application.Services;
using RMS.Modules.Settings.Domain;

namespace RMS.Modules.Settings.Application;

/// <summary>
/// Projects the raw key/value settings dictionary into the strongly typed
/// <see cref="SettingsModel"/> and back into key/value pairs for persistence.
/// Missing values fall back to the catalog defaults.
/// </summary>
public static class SettingsModelMapper
{
    public static SettingsModel ToModel(IReadOnlyDictionary<string, string?> values, IFolderResolver resolver)
    {
        var def = SettingCatalog.Defaults.ToDictionary(d => d.Key, d => d);

        string Str(string key) => values.TryGetValue(key, out var v) && v is not null ? v : (def[key].DefaultValue);
        bool Bool(string key) => Str(key) == "true";
        int Int(string key) => int.TryParse(Str(key), out var n) ? n : 0;
        decimal Dec(string key) => decimal.TryParse(Str(key), out var n) ? n : 0;

        var model = new SettingsModel
        {
            General = new GeneralSettingsModel
            {
                StoreName = Str(SettingCatalog.Keys.GeneralStoreName),
                PhoneNumber = Str(SettingCatalog.Keys.GeneralPhoneNumber),
                Email = Str(SettingCatalog.Keys.GeneralEmail),
                Website = Str(SettingCatalog.Keys.GeneralWebsite),
                TaxNumber = Str(SettingCatalog.Keys.GeneralTaxNumber),
                Currency = Str(SettingCatalog.Keys.GeneralCurrency),
                TimeZone = Str(SettingCatalog.Keys.GeneralTimeZone),
                Language = Str(SettingCatalog.Keys.GeneralLanguage),
                DateFormat = Str(SettingCatalog.Keys.GeneralDateFormat),
                TimeFormat = Str(SettingCatalog.Keys.GeneralTimeFormat),
                NumberFormat = Str(SettingCatalog.Keys.GeneralNumberFormat)
            },
            Store = new StoreSettingsModel
            {
                BusinessLogo = Str(SettingCatalog.Keys.StoreBusinessLogo),
                CompanyAddress = Str(SettingCatalog.Keys.StoreCompanyAddress)
            },
            Receipt = new ReceiptSettingsModel
            {
                Header = Str(SettingCatalog.Keys.ReceiptHeader),
                Footer = Str(SettingCatalog.Keys.ReceiptFooter),
                StoreLogo = Str(SettingCatalog.Keys.ReceiptStoreLogo),
                ShowTaxNumber = Bool(SettingCatalog.Keys.ReceiptShowTaxNumber),
                ShowCashier = Bool(SettingCatalog.Keys.ReceiptShowCashier),
                ShowBarcode = Bool(SettingCatalog.Keys.ReceiptShowBarcode),
                ShowQrCode = Bool(SettingCatalog.Keys.ReceiptShowQrCode),
                PaperWidth = Int(SettingCatalog.Keys.ReceiptPaperWidth),
                AutomaticPrinting = Bool(SettingCatalog.Keys.ReceiptAutomaticPrinting)
            },
            Sales = new SalesSettingsModel
            {
                DefaultTaxRate = Dec(SettingCatalog.Keys.SalesDefaultTaxRate),
                DefaultDiscount = Dec(SettingCatalog.Keys.SalesDefaultDiscount),
                MaximumDiscount = Dec(SettingCatalog.Keys.SalesMaximumDiscount),
                AllowManualPriceChange = Bool(SettingCatalog.Keys.SalesAllowManualPriceChange),
                RequireManagerApproval = Bool(SettingCatalog.Keys.SalesRequireManagerApproval),
                DefaultPaymentMethod = Str(SettingCatalog.Keys.SalesDefaultPaymentMethod),
                AutoCompleteSale = Bool(SettingCatalog.Keys.SalesAutoCompleteSale),
                ReceiptPreview = Bool(SettingCatalog.Keys.SalesReceiptPreview)
            },
            Inventory = new InventorySettingsModel
            {
                DefaultLowStockThreshold = Int(SettingCatalog.Keys.InventoryDefaultLowStockThreshold),
                AllowNegativeStock = Bool(SettingCatalog.Keys.InventoryAllowNegativeStock),
                AutomaticStockAlerts = Bool(SettingCatalog.Keys.InventoryAutomaticStockAlerts),
                RequireConfirmationForAdjustments = Bool(SettingCatalog.Keys.InventoryRequireConfirmationForAdjustments),
                DefaultWarehouse = Str(SettingCatalog.Keys.InventoryDefaultWarehouse),
                DefaultStockAdjustmentReason = Str(SettingCatalog.Keys.InventoryDefaultStockAdjustmentReason)
            },
            Purchasing = new PurchasingSettingsModel
            {
                DefaultSupplier = Str(SettingCatalog.Keys.PurchasingDefaultSupplier),
                PurchaseNumberPrefix = Str(SettingCatalog.Keys.PurchasingPurchaseNumberPrefix),
                AutomaticGoodsReceipt = Bool(SettingCatalog.Keys.PurchasingAutomaticGoodsReceipt),
                DefaultPaymentTerms = Str(SettingCatalog.Keys.PurchasingDefaultPaymentTerms)
            },
            Report = new ReportSettingsModel
            {
                DefaultReportFolder = resolver.Resolve(Str(SettingCatalog.Keys.ReportDefaultReportFolder), "Reports"),
                FileNamePattern = Str(SettingCatalog.Keys.ReportFileNamePattern),
                PdfQuality = Str(SettingCatalog.Keys.ReportPdfQuality),
                CsvDelimiter = Str(SettingCatalog.Keys.ReportCsvDelimiter),
                ExcelExportFormat = Str(SettingCatalog.Keys.ReportExcelExportFormat),
                PrintOrientation = Str(SettingCatalog.Keys.ReportPrintOrientation),
                IncludeCompanyLogo = Bool(SettingCatalog.Keys.ReportIncludeCompanyLogo),
                IncludeReportFooter = Bool(SettingCatalog.Keys.ReportIncludeReportFooter),
                AutomaticNumbering = Bool(SettingCatalog.Keys.ReportAutomaticNumbering)
            },
            Backup = new BackupSettingsModel
            {
                AutomaticBackup = Bool(SettingCatalog.Keys.BackupAutomaticBackup),
                Frequency = Str(SettingCatalog.Keys.BackupFrequency),
                Time = Str(SettingCatalog.Keys.BackupTime),
                MaximumCount = Int(SettingCatalog.Keys.BackupMaximumCount),
                Compress = Bool(SettingCatalog.Keys.BackupCompress),
                VerifyIntegrity = Bool(SettingCatalog.Keys.BackupVerifyIntegrity)
            },
            Application = new ApplicationSettingsModel
            {
                Theme = Str(SettingCatalog.Keys.ApplicationTheme),
                StartupPage = Str(SettingCatalog.Keys.ApplicationStartupPage),
                RememberLastUser = Bool(SettingCatalog.Keys.ApplicationRememberLastUser),
                AutoSave = Bool(SettingCatalog.Keys.ApplicationAutoSave),
                SessionTimeout = Int(SettingCatalog.Keys.ApplicationSessionTimeout)
            },
            Printer = new PrinterSettingsModel
            {
                DefaultPrinter = Str(SettingCatalog.Keys.PrinterDefault),
                ReceiptPrinter = Str(SettingCatalog.Keys.PrinterReceipt),
                InvoicePrinter = Str(SettingCatalog.Keys.PrinterInvoice),
                LabelPrinter = Str(SettingCatalog.Keys.PrinterLabel),
                ReportPrinter = Str(SettingCatalog.Keys.PrinterReport),
                AutoPrint = Bool(SettingCatalog.Keys.PrinterAutoPrint),
                Copies = Int(SettingCatalog.Keys.PrinterCopies),
                PaperWidth = Int(SettingCatalog.Keys.PrinterPaperWidth),
                Orientation = Str(SettingCatalog.Keys.PrinterOrientation),
                MarginMm = Int(SettingCatalog.Keys.PrinterMarginMm),
                CutPaper = Bool(SettingCatalog.Keys.PrinterCutPaper),
                OpenDrawer = Bool(SettingCatalog.Keys.PrinterOpenDrawer)
            }
        };

        model.Storage = SettingCatalog.FolderDefinitions
            .Select(f => new FolderSettingModel
            {
                Key = f.Key,
                DisplayName = f.DisplayName,
                Path = resolver.Resolve(values.TryGetValue(f.Key, out var v) ? v : f.DefaultValue, f.FolderSubPath),
                DefaultPath = resolver.GetDefaultPath(f.FolderSubPath!)
            })
            .ToList();

        return model;
    }

    public static Dictionary<string, string?> GeneralPairs(GeneralSettingsModel m) => new()
    {
        [SettingCatalog.Keys.GeneralStoreName] = m.StoreName,
        [SettingCatalog.Keys.GeneralPhoneNumber] = m.PhoneNumber,
        [SettingCatalog.Keys.GeneralEmail] = m.Email,
        [SettingCatalog.Keys.GeneralWebsite] = m.Website,
        [SettingCatalog.Keys.GeneralTaxNumber] = m.TaxNumber,
        [SettingCatalog.Keys.GeneralCurrency] = m.Currency,
        [SettingCatalog.Keys.GeneralTimeZone] = m.TimeZone,
        [SettingCatalog.Keys.GeneralLanguage] = m.Language,
        [SettingCatalog.Keys.GeneralDateFormat] = m.DateFormat,
        [SettingCatalog.Keys.GeneralTimeFormat] = m.TimeFormat,
        [SettingCatalog.Keys.GeneralNumberFormat] = m.NumberFormat
    };

    public static Dictionary<string, string?> StorePairs(StoreSettingsModel m) => new()
    {
        [SettingCatalog.Keys.StoreBusinessLogo] = m.BusinessLogo,
        [SettingCatalog.Keys.StoreCompanyAddress] = m.CompanyAddress
    };

    public static Dictionary<string, string?> ReceiptPairs(ReceiptSettingsModel m) => new()
    {
        [SettingCatalog.Keys.ReceiptHeader] = m.Header,
        [SettingCatalog.Keys.ReceiptFooter] = m.Footer,
        [SettingCatalog.Keys.ReceiptStoreLogo] = m.StoreLogo,
        [SettingCatalog.Keys.ReceiptShowTaxNumber] = m.ShowTaxNumber.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.ReceiptShowCashier] = m.ShowCashier.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.ReceiptShowBarcode] = m.ShowBarcode.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.ReceiptShowQrCode] = m.ShowQrCode.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.ReceiptPaperWidth] = m.PaperWidth.ToString(),
        [SettingCatalog.Keys.ReceiptAutomaticPrinting] = m.AutomaticPrinting.ToString().ToLowerInvariant()
    };

    public static Dictionary<string, string?> SalesPairs(SalesSettingsModel m) => new()
    {
        [SettingCatalog.Keys.SalesDefaultTaxRate] = m.DefaultTaxRate.ToString(),
        [SettingCatalog.Keys.SalesDefaultDiscount] = m.DefaultDiscount.ToString(),
        [SettingCatalog.Keys.SalesMaximumDiscount] = m.MaximumDiscount.ToString(),
        [SettingCatalog.Keys.SalesAllowManualPriceChange] = m.AllowManualPriceChange.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.SalesRequireManagerApproval] = m.RequireManagerApproval.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.SalesDefaultPaymentMethod] = m.DefaultPaymentMethod,
        [SettingCatalog.Keys.SalesAutoCompleteSale] = m.AutoCompleteSale.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.SalesReceiptPreview] = m.ReceiptPreview.ToString().ToLowerInvariant()
    };

    public static Dictionary<string, string?> InventoryPairs(InventorySettingsModel m) => new()
    {
        [SettingCatalog.Keys.InventoryDefaultLowStockThreshold] = m.DefaultLowStockThreshold.ToString(),
        [SettingCatalog.Keys.InventoryAllowNegativeStock] = m.AllowNegativeStock.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.InventoryAutomaticStockAlerts] = m.AutomaticStockAlerts.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.InventoryRequireConfirmationForAdjustments] = m.RequireConfirmationForAdjustments.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.InventoryDefaultWarehouse] = m.DefaultWarehouse,
        [SettingCatalog.Keys.InventoryDefaultStockAdjustmentReason] = m.DefaultStockAdjustmentReason
    };

    public static Dictionary<string, string?> PurchasingPairs(PurchasingSettingsModel m) => new()
    {
        [SettingCatalog.Keys.PurchasingDefaultSupplier] = m.DefaultSupplier,
        [SettingCatalog.Keys.PurchasingPurchaseNumberPrefix] = m.PurchaseNumberPrefix,
        [SettingCatalog.Keys.PurchasingAutomaticGoodsReceipt] = m.AutomaticGoodsReceipt.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.PurchasingDefaultPaymentTerms] = m.DefaultPaymentTerms
    };

    public static Dictionary<string, string?> ReportPairs(ReportSettingsModel m, IFolderResolver resolver) => new()
    {
        [SettingCatalog.Keys.ReportDefaultReportFolder] = resolver.GetRelativeOrAbsolute(m.DefaultReportFolder),
        [SettingCatalog.Keys.ReportFileNamePattern] = m.FileNamePattern,
        [SettingCatalog.Keys.ReportPdfQuality] = m.PdfQuality,
        [SettingCatalog.Keys.ReportCsvDelimiter] = m.CsvDelimiter,
        [SettingCatalog.Keys.ReportExcelExportFormat] = m.ExcelExportFormat,
        [SettingCatalog.Keys.ReportPrintOrientation] = m.PrintOrientation,
        [SettingCatalog.Keys.ReportIncludeCompanyLogo] = m.IncludeCompanyLogo.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.ReportIncludeReportFooter] = m.IncludeReportFooter.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.ReportAutomaticNumbering] = m.AutomaticNumbering.ToString().ToLowerInvariant()
    };

    public static Dictionary<string, string?> BackupPairs(BackupSettingsModel m) => new()
    {
        [SettingCatalog.Keys.BackupAutomaticBackup] = m.AutomaticBackup.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.BackupFrequency] = m.Frequency,
        [SettingCatalog.Keys.BackupTime] = m.Time,
        [SettingCatalog.Keys.BackupMaximumCount] = m.MaximumCount.ToString(),
        [SettingCatalog.Keys.BackupCompress] = m.Compress.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.BackupVerifyIntegrity] = m.VerifyIntegrity.ToString().ToLowerInvariant()
    };

    public static Dictionary<string, string?> ApplicationPairs(ApplicationSettingsModel m) => new()
    {
        [SettingCatalog.Keys.ApplicationTheme] = m.Theme,
        [SettingCatalog.Keys.ApplicationStartupPage] = m.StartupPage,
        [SettingCatalog.Keys.ApplicationRememberLastUser] = m.RememberLastUser.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.ApplicationAutoSave] = m.AutoSave.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.ApplicationSessionTimeout] = m.SessionTimeout.ToString()
    };

    public static Dictionary<string, string?> PrinterPairs(PrinterSettingsModel m) => new()
    {
        [SettingCatalog.Keys.PrinterDefault] = m.DefaultPrinter,
        [SettingCatalog.Keys.PrinterReceipt] = m.ReceiptPrinter,
        [SettingCatalog.Keys.PrinterInvoice] = m.InvoicePrinter,
        [SettingCatalog.Keys.PrinterLabel] = m.LabelPrinter,
        [SettingCatalog.Keys.PrinterReport] = m.ReportPrinter,
        [SettingCatalog.Keys.PrinterAutoPrint] = m.AutoPrint.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.PrinterCopies] = m.Copies.ToString(),
        [SettingCatalog.Keys.PrinterPaperWidth] = m.PaperWidth.ToString(),
        [SettingCatalog.Keys.PrinterOrientation] = m.Orientation,
        [SettingCatalog.Keys.PrinterMarginMm] = m.MarginMm.ToString(),
        [SettingCatalog.Keys.PrinterCutPaper] = m.CutPaper.ToString().ToLowerInvariant(),
        [SettingCatalog.Keys.PrinterOpenDrawer] = m.OpenDrawer.ToString().ToLowerInvariant()
    };
}
