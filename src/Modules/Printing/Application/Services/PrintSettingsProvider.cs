using RMS.BuildingBlocks.Results;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Application.Models;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;
using RMS.Modules.Settings.Application.Contracts;
using RMS.Modules.Settings.Domain;

namespace RMS.Modules.Printing.Application.Services;

/// <summary>
/// Reads printer + branding configuration from the canonical Settings module so
/// printing never duplicates or owns its own copy of those settings.
/// </summary>
public sealed class PrintSettingsProvider : IPrintSettingsProvider
{
    private readonly ISettingsReadStore _readStore;

    public PrintSettingsProvider(ISettingsReadStore readStore) => _readStore = readStore;

    public async Task<PrintSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        var values = await _readStore.GetAllValuesAsync(cancellationToken);
        var def = SettingCatalog.Defaults.ToDictionary(d => d.Key, d => d);

        string Str(string key) => values.TryGetValue(key, out var v) && v is not null ? v : def[key].DefaultValue;
        bool Bool(string key) => Str(key) == "true";
        int Int(string key) => int.TryParse(Str(key), out var n) ? n : 0;

        var branding = new BrandingInfo(
            StoreName: Str(SettingCatalog.Keys.GeneralStoreName),
            Address: Str(SettingCatalog.Keys.StoreCompanyAddress),
            Phone: Str(SettingCatalog.Keys.GeneralPhoneNumber),
            TaxNumber: Str(SettingCatalog.Keys.GeneralTaxNumber),
            Email: Str(SettingCatalog.Keys.GeneralEmail),
            Website: Str(SettingCatalog.Keys.GeneralWebsite),
            LogoPath: string.IsNullOrWhiteSpace(Str(SettingCatalog.Keys.ReceiptStoreLogo))
                ? Str(SettingCatalog.Keys.StoreBusinessLogo)
                : Str(SettingCatalog.Keys.ReceiptStoreLogo),
            ReceiptHeader: Str(SettingCatalog.Keys.ReceiptHeader),
            ReceiptFooter: Str(SettingCatalog.Keys.ReceiptFooter),
            CurrencyCode: Str(SettingCatalog.Keys.GeneralCurrency));

        var settings = new PrintSettings(
            DefaultPrinter: Str(SettingCatalog.Keys.PrinterDefault),
            ReceiptPrinter: Str(SettingCatalog.Keys.PrinterReceipt),
            InvoicePrinter: Str(SettingCatalog.Keys.PrinterInvoice),
            LabelPrinter: Str(SettingCatalog.Keys.PrinterLabel),
            ReportPrinter: Str(SettingCatalog.Keys.PrinterReport),
            AutoPrint: Bool(SettingCatalog.Keys.PrinterAutoPrint),
            Copies: Int(SettingCatalog.Keys.PrinterCopies),
            PaperWidthMm: Int(SettingCatalog.Keys.PrinterPaperWidth),
            Orientation: Str(SettingCatalog.Keys.PrinterOrientation) is "Landscape" ? PrintOrientation.Landscape : PrintOrientation.Portrait,
            MarginMm: Int(SettingCatalog.Keys.PrinterMarginMm),
            CutPaper: Bool(SettingCatalog.Keys.PrinterCutPaper),
            OpenDrawer: Bool(SettingCatalog.Keys.PrinterOpenDrawer),
            Branding: branding);

        return settings;
    }
}
