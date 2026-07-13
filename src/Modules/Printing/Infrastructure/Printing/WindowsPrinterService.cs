using System.Diagnostics;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Application.Models;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Infrastructure.Printing;

/// <summary>
/// Windows/network printer adapter. Discovers installed printers and prints PDF
/// documents through the shell "print" verb, or sends raw ESC/POS streams to a
/// named printer via the spooler.
/// </summary>
public sealed class WindowsPrinterService : IPrinterService
{
    private readonly IPrinterDiscovery _discovery;

    public WindowsPrinterService(IPrinterDiscovery discovery) => _discovery = discovery;

    public IReadOnlyList<PrinterDescriptor> DiscoverPrinters() => _discovery.DiscoverPrinters();

    public PrinterDescriptor? GetDefaultPrinter()
    {
        var printers = _discovery.DiscoverPrinters();
        var def = _discovery.GetDefaultPrinterName();
        return printers.FirstOrDefault(p => p.Name == def) ?? printers.FirstOrDefault();
    }

    public Result<PrinterStatus> GetStatus(string printerName)
    {
        var printer = _discovery.DiscoverPrinters().FirstOrDefault(p => p.Name == printerName);
        if (printer is null)
            return Result.Failure<PrinterStatus>($"The printer '{printerName}' was not found.", "PRINTER_NOT_FOUND");
        return Result.Success(printer.Status);
    }

    public async Task<Result> PrintPdfAsync(string printerName, byte[] pdf, PrintOptions options, CancellationToken cancellationToken = default)
    {
        if (_discovery.DiscoverPrinters().All(p => p.Name != printerName))
            return Result.Failure($"The printer '{printerName}' was not found.", "PRINTER_NOT_FOUND");

        try
        {
            var folder = Path.Combine(Path.GetTempPath(), "RMS");
            Directory.CreateDirectory(folder);
            var tempPath = Path.Combine(folder, $"print_{Guid.NewGuid():N}.pdf");
            await File.WriteAllBytesAsync(tempPath, pdf, cancellationToken);

            var psi = new ProcessStartInfo(tempPath)
            {
                Verb = "print",
                CreateNoWindow = true,
                UseShellExecute = true
            };

            using var process = Process.Start(psi);
            if (process is not null)
            {
                try
                {
                    await process.WaitForExitAsync(cancellationToken)
                        .WaitAsync(TimeSpan.FromSeconds(20), cancellationToken);
                }
                catch (Exception)
                {
                    // Shell print may be asynchronous; proceed. The spooler handles delivery.
                }
            }

            return Result.Success();
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 5)
        {
            return Result.Failure($"Access to the printer '{printerName}' was denied.", "PRINTER_ACCESS_DENIED");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Printing to '{printerName}' failed: {ex.Message}", "PRINT_FAILED");
        }
    }

    public Task<Result> PrintRawAsync(string printerName, byte[] rawBytes, CancellationToken cancellationToken = default)
    {
        try
        {
            EscPos.RawPrinterHelper.Send(printerName, rawBytes);
            return Task.FromResult(Result.Success());
        }
        catch (RMS.Modules.Printing.Domain.PrintingException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message, ex.ErrorCode));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure($"Raw print to '{printerName}' failed: {ex.Message}", "PRINT_FAILED"));
        }
    }
}
