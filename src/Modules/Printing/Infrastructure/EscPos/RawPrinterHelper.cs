using System.Runtime.InteropServices;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Infrastructure.EscPos;

/// <summary>Sends raw ESC/POS byte streams to a Windows-installed printer via the spooler.</summary>
internal static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DocInfo
    {
        public string pDocName;
        public string pOutputFile;
        public string pDataType;
    }

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DocInfo di);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    public static void Send(string printerName, byte[] data)
    {
        if (!OperatingSystem.IsWindows())
            throw new InvalidPrinterException("Raw (ESC/POS) printing is only supported on Windows.");

        IntPtr hPrinter = IntPtr.Zero;
        var di = new DocInfo { pDocName = "RMS Receipt", pOutputFile = null!, pDataType = "RAW" };

        try
        {
            if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                throw new PrinterNotFoundException(printerName);
            if (!StartDocPrinter(hPrinter, 1, ref di))
                throw new PrintFailureException(printerName, "Could not start the print document.");
            if (!StartPagePrinter(hPrinter))
                throw new PrintFailureException(printerName, "Could not start the print page.");

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                if (!WritePrinter(hPrinter, handle.AddrOfPinnedObject(), data.Length, out _))
                    throw new PrintFailureException(printerName, "Writing to the printer failed.");
            }
            finally
            {
                handle.Free();
            }

            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
        }
        finally
        {
            if (hPrinter != IntPtr.Zero) ClosePrinter(hPrinter);
        }
    }
}
