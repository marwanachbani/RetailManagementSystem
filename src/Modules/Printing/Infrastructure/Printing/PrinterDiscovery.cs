using Microsoft.Win32;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Domain;

namespace RMS.Modules.Printing.Infrastructure.Printing;

/// <summary>
/// Discovers installed Windows printers via the registry (no external dependency).
/// On non-Windows hosts it returns an empty list.
/// </summary>
public sealed class PrinterDiscovery : IPrinterDiscovery
{
    private const string PrintersKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Print\Printers";
    private const string WindowsKey = @"Software\Microsoft\Windows NT\CurrentVersion\Windows";
    private const int AttributeShared = 0x8;

    public IReadOnlyList<PrinterDescriptor> DiscoverPrinters()
    {
        var list = new List<PrinterDescriptor>();
        if (!OperatingSystem.IsWindows()) return list;

        try
        {
            using var root = Registry.LocalMachine.OpenSubKey(PrintersKey);
            if (root is null) return list;

            foreach (var name in root.GetSubKeyNames())
            {
                using var pk = root.OpenSubKey(name);
                if (pk is null) continue;

                var display = pk.GetValue("Name") as string ?? name;
                var port = pk.GetValue("Port") as string ?? string.Empty;
                var attributes = (int)(pk.GetValue("Attributes") ?? 0);
                var status = (int)(pk.GetValue("Status") ?? 0);
                var kind = InferKind(display);
                var width = display.Contains("58") ? 58m : 80m;

                list.Add(new PrinterDescriptor(
                    display,
                    kind,
                    false,
                    status == 0 ? PrinterStatus.Ready : PrinterStatus.Error,
                    width,
                    Location: port,
                    IsShared: (attributes & AttributeShared) != 0));
            }
        }
        catch
        {
            // Discovery is best-effort; never throw from enumeration.
        }

        return list;
    }

    public string? GetDefaultPrinterName()
    {
        if (!OperatingSystem.IsWindows()) return null;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(WindowsKey);
            var device = key?.GetValue("Device") as string;
            if (!string.IsNullOrWhiteSpace(device))
                return device.Split(',')[0];
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static PrinterKind InferKind(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.Contains("thermal") || n.Contains("pos") || n.Contains("receipt") || n.Contains("epos") || n.Contains("tm-"))
            return PrinterKind.ThermalPos;
        return PrinterKind.Windows;
    }
}
