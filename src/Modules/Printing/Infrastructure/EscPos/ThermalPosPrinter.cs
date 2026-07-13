using RMS.BuildingBlocks.Results;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;
using ZXing;
using ZXing.Common;
using Result = RMS.BuildingBlocks.Results.Result;

namespace RMS.Modules.Printing.Infrastructure.EscPos;

/// <summary>
/// Prints receipts and labels to ESC/POS thermal printers (58 mm / 80 mm, USB,
/// network or Windows-installed) by emitting raw command streams.
/// </summary>
public sealed class ThermalPosPrinter : IReceiptPrinter, ILabelPrinter
{
    private readonly IPrintSettingsProvider? _settings;

    public ThermalPosPrinter(IPrintSettingsProvider? settings = null) => _settings = settings;

    public async Task<Result> PrintReceiptAsync(
        ReceiptDocument receipt, BrandingInfo branding, string printerName, CancellationToken cancellationToken = default)
    {
        try
        {
            var opts = _settings is not null
                ? await _settings.GetAsync(cancellationToken)
                : null;

            var b = new EscPosBuilder().Reset();
            b.Align(TextAlign.Center).Bold().Line(branding.StoreName).Bold(false);
            if (!string.IsNullOrWhiteSpace(branding.Address)) b.Line(branding.Address);
            if (!string.IsNullOrWhiteSpace(branding.Phone)) b.Line(branding.Phone);
            if (!string.IsNullOrWhiteSpace(branding.TaxNumber)) b.Line("Tax No: " + branding.TaxNumber);
            if (!string.IsNullOrWhiteSpace(branding.ReceiptHeader)) b.Line(branding.ReceiptHeader);
            b.Line(new string('-', 32));

            b.Align(TextAlign.Left)
                .Line($"Receipt: {receipt.ReceiptNumber}")
                .Line($"Date: {receipt.SaleDate:yyyy-MM-dd HH:mm}")
                .Line($"Cashier: {receipt.CashierName}");
            if (!string.IsNullOrWhiteSpace(receipt.CustomerName))
                b.Line($"Customer: {receipt.CustomerName}");
            b.Line(new string('-', 32));

            foreach (var it in receipt.Items)
                b.Line($"{it.Name} x{it.Quantity}  {it.LineTotal:F2}");
            b.Line(new string('-', 32));

            if (receipt.Totals is not null)
            {
                b.Line($"Subtotal: {receipt.Totals.SubTotal:F2}");
                if (receipt.Totals.DiscountTotal != 0) b.Line($"Discount: -{receipt.Totals.DiscountTotal:F2}");
                if (receipt.Totals.TaxTotal != 0) b.Line($"Tax: {receipt.Totals.TaxTotal:F2}");
                b.Bold().Line($"TOTAL: {receipt.Totals.GrandTotal:F2}").Bold(false);
                if (receipt.Totals.PaidAmount.HasValue) b.Line($"Paid: {receipt.Totals.PaidAmount:F2}");
                if (receipt.Totals.Change.HasValue) b.Line($"Change: {receipt.Totals.Change:F2}");
            }

            if (!string.IsNullOrWhiteSpace(receipt.BarcodeData))
                b.Align(TextAlign.Center).RasterImage(ToMatrix(receipt.BarcodeData!, BarcodeSymbology.Code128, 256, 80)).Line();
            if (!string.IsNullOrWhiteSpace(receipt.QrData))
                b.Align(TextAlign.Center).RasterImage(ToMatrix(receipt.QrData!, BarcodeSymbology.QRCode, 200, 200)).Line();

            b.Align(TextAlign.Center).Line(receipt.ThankYouMessage);
            if (!string.IsNullOrWhiteSpace(receipt.FooterText)) b.Line(receipt.FooterText);

            if (opts?.OpenDrawer == true) b.OpenDrawer();
            if (opts?.CutPaper != false) b.FeedAndCut(3);

            RawPrinterHelper.Send(printerName, b.Build());
            return Result.Success();
        }
        catch (PrintingException ex)
        {
            return Result.Failure(ex.Message, ex.ErrorCode);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Thermal print failed: {ex.Message}", "PRINT_FAILED");
        }
    }

    public async Task<Result> PrintLabelsAsync(
        IEnumerable<LabelItem> labels, DocumentType labelType, BrandingInfo branding, string printerName, CancellationToken cancellationToken = default)
    {
        try
        {
            var opts = _settings is not null ? await _settings.GetAsync(cancellationToken) : null;
            var list = labels.ToList();

            var b = new EscPosBuilder().Reset();
            foreach (var item in list)
            {
                b.Align(TextAlign.Center);
                if (!string.IsNullOrWhiteSpace(item.Name)) b.Bold().Line(item.Name).Bold(false);
                b.RasterImage(ToMatrix(item.BarcodeValue, item.Symbology, 256, item.Symbology == BarcodeSymbology.QRCode ? 200 : 80)).Line();
                if (!string.IsNullOrWhiteSpace(item.Price)) b.Line(item.Price);
                if (!string.IsNullOrWhiteSpace(item.Sku)) b.Line(item.Sku);
                b.Line();
            }

            if (opts?.OpenDrawer == true) b.OpenDrawer();
            if (opts?.CutPaper != false) b.FeedAndCut(3);

            RawPrinterHelper.Send(printerName, b.Build());
            return Result.Success();
        }
        catch (PrintingException ex)
        {
            return Result.Failure(ex.Message, ex.ErrorCode);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Thermal label print failed: {ex.Message}", "PRINT_FAILED");
        }
    }

    private static bool[,] ToMatrix(string content, BarcodeSymbology symbology, int width, int height)
    {
        var matrix = new MultiFormatWriter().encode(content, ToFormat(symbology), width, height);
        var result = new bool[matrix.Height, matrix.Width];
        for (var y = 0; y < matrix.Height; y++)
            for (var x = 0; x < matrix.Width; x++)
                result[y, x] = matrix[x, y];
        return result;
    }

    private static BarcodeFormat ToFormat(BarcodeSymbology symbology) => symbology switch
    {
        BarcodeSymbology.EAN13 => BarcodeFormat.EAN_13,
        BarcodeSymbology.Code128 => BarcodeFormat.CODE_128,
        BarcodeSymbology.Code39 => BarcodeFormat.CODE_39,
        _ => BarcodeFormat.QR_CODE
    };
}
