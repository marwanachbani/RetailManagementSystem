namespace RMS.Modules.Printing.Domain;

/// <summary>
/// Base class for every printing failure. Carries a stable <see cref="ErrorCode"/>
/// so the WPF layer can show user-friendly, localised messages.
/// </summary>
public abstract class PrintingException : Exception
{
    protected PrintingException(string errorCode, string message, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}

public sealed class PrinterNotFoundException : PrintingException
{
    public PrinterNotFoundException(string printerName)
        : base("PRINTER_NOT_FOUND", $"The printer '{printerName}' could not be found. Install it or pick another printer.") { }

    public string PrinterName { get; } = string.Empty;
}

public sealed class PrinterOfflineException : PrintingException
{
    public PrinterOfflineException(string printerName)
        : base("PRINTER_OFFLINE", $"The printer '{printerName}' is offline. Check the connection and try again.") { }
}

public sealed class PaperEmptyException : PrintingException
{
    public PaperEmptyException(string printerName)
        : base("PRINTER_NO_PAPER", $"The printer '{printerName}' is out of paper. Load paper and try again.") { }
}

public sealed class InvalidPrinterException : PrintingException
{
    public InvalidPrinterException(string message) : base("PRINTER_INVALID", message) { }
}

public sealed class PrintAccessException : PrintingException
{
    public PrintAccessException(string printerName, string detail)
        : base("PRINTER_ACCESS_DENIED", $"Access to the printer '{printerName}' was denied. {detail}") { }
}

public sealed class PrintFailureException : PrintingException
{
    public PrintFailureException(string printerName, string detail)
        : base("PRINT_FAILED", $"Printing to '{printerName}' failed. {detail}") { }
}
