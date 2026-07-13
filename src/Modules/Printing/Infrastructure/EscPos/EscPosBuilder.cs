using System.Text;

namespace RMS.Modules.Printing.Infrastructure.EscPos;

/// <summary>
/// Fluent builder for ESC/POS command streams understood by thermal POS printers
/// (58 mm and 80 mm, USB / network / Windows-installed). Includes text formatting,
/// rasterised barcodes/QR codes (via bit-image), paper cut and cash-drawer kick.
/// </summary>
public sealed class EscPosBuilder
{
    private readonly List<byte> _buffer = new();

    public static readonly byte[] Initialize = { 0x1B, 0x40 };
    private static readonly byte[] AlignLeft = { 0x1B, 0x61, 0x00 };
    private static readonly byte[] AlignCenter = { 0x1B, 0x61, 0x01 };
    private static readonly byte[] AlignRight = { 0x1B, 0x61, 0x02 };
    private static readonly byte[] BoldOn = { 0x1B, 0x45, 0x01 };
    private static readonly byte[] BoldOff = { 0x1B, 0x45, 0x00 };
    private static readonly byte[] FeedLine = { 0x0A };

    public EscPosBuilder Reset()
    {
        _buffer.AddRange(Initialize);
        return this;
    }

    public EscPosBuilder Align(TextAlign align)
    {
        _buffer.AddRange(align switch
        {
            TextAlign.Center => AlignCenter,
            TextAlign.Right => AlignRight,
            _ => AlignLeft
        });
        return this;
    }

    public EscPosBuilder Bold(bool on = true)
    {
        _buffer.AddRange(on ? BoldOn : BoldOff);
        return this;
    }

    public EscPosBuilder Text(string? text)
    {
        if (string.IsNullOrEmpty(text)) return this;
        var bytes = Encoding.ASCII.GetBytes(Normalize(text!));
        _buffer.AddRange(bytes);
        return this;
    }

    public EscPosBuilder Line(string? text = null)
    {
        if (!string.IsNullOrEmpty(text)) Text(text);
        _buffer.AddRange(FeedLine);
        return this;
    }

    public EscPosBuilder Feed(int lines = 1)
    {
        for (var i = 0; i < lines; i++) _buffer.AddRange(FeedLine);
        return this;
    }

    /// <summary>Renders a monochrome bit matrix (e.g. a barcode or QR code) as an ESC/POS raster image.</summary>
    public EscPosBuilder RasterImage(bool[,] matrix)
    {
        var height = matrix.GetLength(0);
        var width = matrix.GetLength(1);
        var widthBytes = (width + 7) / 8;

        _buffer.Add(0x1D);
        _buffer.Add(0x76);
        _buffer.Add(0x30);
        _buffer.Add(0x00);
        _buffer.Add((byte)(widthBytes & 0xFF));
        _buffer.Add((byte)((widthBytes >> 8) & 0xFF));
        _buffer.Add((byte)(height & 0xFF));
        _buffer.Add((byte)((height >> 8) & 0xFF));

        for (var y = 0; y < height; y++)
        {
            for (var xByte = 0; xByte < widthBytes; xByte++)
            {
                byte b = 0;
                for (var bit = 0; bit < 8; bit++)
                {
                    var x = xByte * 8 + (7 - bit);
                    if (x < width && matrix[y, x]) b |= (byte)(1 << bit);
                }

                _buffer.Add(b);
            }
        }

        return this;
    }

    public EscPosBuilder Cut()
    {
        // GS V 0  -> full cut (feed 0)
        _buffer.Add(0x1D);
        _buffer.Add(0x56);
        _buffer.Add(0x00);
        return this;
    }

    public EscPosBuilder FeedAndCut(int lines = 3)
    {
        Feed(lines);
        Cut();
        return this;
    }

    public EscPosBuilder OpenDrawer()
    {
        // ESC p m t1 t2  (pulse on connector pin 2)
        _buffer.Add(0x1B);
        _buffer.Add(0x70);
        _buffer.Add(0x00);
        _buffer.Add(0x19);
        _buffer.Add(0xFA);
        return this;
    }

    public byte[] Build() => _buffer.ToArray();

    private static string Normalize(string text)
    {
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] > 127) chars[i] = '?';
        }

        return new string(chars);
    }
}

public enum TextAlign
{
    Left,
    Center,
    Right
}
