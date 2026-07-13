using System.IO.Compression;

namespace RMS.Modules.Printing.Infrastructure.Barcode;

/// <summary>
/// Minimal, dependency-free PNG encoder for 32-bit RGBA pixel data. Used to turn
/// ZXing.Net pixel output into a PNG byte array that QuestPDF can embed.
/// </summary>
internal static class PngEncoder
{
    private static readonly uint[] CrcTable = BuildCrcTable();

    public static byte[] EncodeRgba(byte[] rgba, int width, int height)
    {
        var ms = new MemoryStream();
        ms.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

        var ihdr = new MemoryStream();
        WriteInt32(ihdr, width);
        WriteInt32(ihdr, height);
        ihdr.WriteByte(8);  // bit depth
        ihdr.WriteByte(6);  // colour type RGBA
        ihdr.WriteByte(0);  // compression
        ihdr.WriteByte(0);  // filter
        ihdr.WriteByte(0);  // interlace
        WriteChunk(ms, "IHDR", ihdr.ToArray());

        var raw = new MemoryStream();
        for (var y = 0; y < height; y++)
        {
            raw.WriteByte(0); // filter type: none
            raw.Write(rgba, y * width * 4, width * 4);
        }

        WriteChunk(ms, "IDAT", ZlibCompress(raw.ToArray()));
        WriteChunk(ms, "IEND", Array.Empty<byte>());
        return ms.ToArray();
    }

    private static byte[] ZlibCompress(byte[] data)
    {
        var outMs = new MemoryStream();
        outMs.WriteByte(0x78);
        outMs.WriteByte(0x9C);
        using (var deflate = new DeflateStream(outMs, CompressionLevel.Optimal, true))
        {
            deflate.Write(data, 0, data.Length);
        }

        var adler = Adler32(data);
        outMs.Write(adler, 0, adler.Length);
        return outMs.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        WriteInt32(stream, data.Length);
        var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes, 0, typeBytes.Length);
        stream.Write(data, 0, data.Length);

        var crc = new MemoryStream();
        crc.Write(typeBytes, 0, typeBytes.Length);
        crc.Write(data, 0, data.Length);
        WriteInt32(stream, (int)Crc32(crc.ToArray()));
    }

    private static void WriteInt32(Stream stream, int value)
    {
        var bytes = new[]
        {
            (byte)((value >> 24) & 0xFF),
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)(value & 0xFF)
        };
        stream.Write(bytes, 0, 4);
    }

    private static byte[] Adler32(byte[] data)
    {
        const uint mod = 65521;
        uint a = 1, b = 0;
        foreach (var b0 in data)
        {
            a = (a + b0) % mod;
            b = (b + a) % mod;
        }

        var value = (b << 16) | a;
        return new[]
        {
            (byte)((value >> 24) & 0xFF),
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)(value & 0xFF)
        };
    }

    private static uint Crc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var b0 in data)
        {
            crc ^= b0;
            crc = CrcTable[crc & 0xFF] ^ (crc >> 8);
        }

        return crc ^ 0xFFFFFFFF;
    }

    private static uint[] BuildCrcTable()
    {
        const uint poly = 0xEDB88320;
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
                c = (c & 1) != 0 ? poly ^ (c >> 1) : c >> 1;
            table[n] = c;
        }

        return table;
    }
}
