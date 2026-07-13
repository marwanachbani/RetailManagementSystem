using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Domain;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace RMS.Modules.Printing.Infrastructure.Barcode;

/// <summary>
/// Generates barcode and QR-code images using ZXing.Net and encodes the resulting
/// RGBA pixels to PNG with the dependency-free <see cref="PngEncoder"/>.
/// </summary>
public sealed class BarcodeGenerator : IBarcodeGenerator
{
    public byte[] Generate(string content, BarcodeSymbology symbology, int width, int height, bool pureBarcode = true)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidPrinterException("Barcode content must not be empty.");

        var writer = new BarcodeWriterPixelData
        {
            Format = ToFormat(symbology),
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = pureBarcode ? 0 : 4,
                PureBarcode = pureBarcode
            }
        };

        var data = writer.Write(content);
        return PngEncoder.EncodeRgba(data.Pixels, data.Width, data.Height);
    }

    public byte[] GenerateQr(string content, int size) =>
        Generate(content, BarcodeSymbology.QRCode, size, size, true);

    private static BarcodeFormat ToFormat(BarcodeSymbology symbology) => symbology switch
    {
        BarcodeSymbology.EAN13 => BarcodeFormat.EAN_13,
        BarcodeSymbology.Code128 => BarcodeFormat.CODE_128,
        BarcodeSymbology.Code39 => BarcodeFormat.CODE_39,
        _ => BarcodeFormat.QR_CODE
    };
}
