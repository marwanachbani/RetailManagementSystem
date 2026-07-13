using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.Commands;
using RMS.WPF.Services;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Settings;

public sealed class PrinterSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();
    private readonly IPrintingService _printing;

    public PrinterSettingsViewModel(IMediator mediator, IDialogService dialogService, IPrintingService printing)
        : base(mediator, dialogService) => _printing = printing;

    public override string Title => "Printers";
    public override string Description => "Assign printers for each document type and configure thermal options.";

    public ObservableCollection<string> Printers { get; } = new();

    private string _defaultPrinter = string.Empty;
    public string DefaultPrinter { get => _defaultPrinter; set { _defaultPrinter = value; OnPropertyChanged(); } }

    private string _receiptPrinter = string.Empty;
    public string ReceiptPrinter { get => _receiptPrinter; set { _receiptPrinter = value; OnPropertyChanged(); } }

    private string _invoicePrinter = string.Empty;
    public string InvoicePrinter { get => _invoicePrinter; set { _invoicePrinter = value; OnPropertyChanged(); } }

    private string _labelPrinter = string.Empty;
    public string LabelPrinter { get => _labelPrinter; set { _labelPrinter = value; OnPropertyChanged(); } }

    private string _reportPrinter = string.Empty;
    public string ReportPrinter { get => _reportPrinter; set { _reportPrinter = value; OnPropertyChanged(); } }

    private bool _autoPrint;
    public bool AutoPrint { get => _autoPrint; set { _autoPrint = value; OnPropertyChanged(); } }

    private int _copies = 1;
    public int Copies { get => _copies; set { _copies = value; OnPropertyChanged(); } }

    private int _paperWidth = 80;
    public int PaperWidth { get => _paperWidth; set { _paperWidth = value; OnPropertyChanged(); } }

    private string _orientation = "Portrait";
    public string Orientation { get => _orientation; set { _orientation = value; OnPropertyChanged(); } }

    private int _marginMm = 10;
    public int MarginMm { get => _marginMm; set { _marginMm = value; OnPropertyChanged(); } }

    private bool _cutPaper = true;
    public bool CutPaper { get => _cutPaper; set { _cutPaper = value; OnPropertyChanged(); } }

    private bool _openDrawer;
    public bool OpenDrawer { get => _openDrawer; set { _openDrawer = value; OnPropertyChanged(); } }

    public IReadOnlyList<string> Orientations { get; } = new[] { "Portrait", "Landscape" };

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad(); ApplyFrom(model); _snapshot = BuildModel(); EndLoad();
        _ = LoadPrintersAsync();
    }

    private async Task LoadPrintersAsync()
    {
        try
        {
            var printers = await _printing.GetPrintersAsync();
            Printers.Clear();
            Printers.Add(string.Empty);
            foreach (var p in printers)
                Printers.Add(p.Name);
        }
        catch
        {
            // Discovery is best-effort; leave the combo editable.
        }
    }

    private void ApplyFrom(SettingsModel model)
    {
        var p = model.Printer;
        DefaultPrinter = p.DefaultPrinter; ReceiptPrinter = p.ReceiptPrinter; InvoicePrinter = p.InvoicePrinter;
        LabelPrinter = p.LabelPrinter; ReportPrinter = p.ReportPrinter;
        AutoPrint = p.AutoPrint; Copies = p.Copies; PaperWidth = p.PaperWidth;
        Orientation = p.Orientation; MarginMm = p.MarginMm; CutPaper = p.CutPaper; OpenDrawer = p.OpenDrawer;
    }

    private SettingsModel BuildModel() => new()
    {
        Printer = new PrinterSettingsModel
        {
            DefaultPrinter = DefaultPrinter, ReceiptPrinter = ReceiptPrinter, InvoicePrinter = InvoicePrinter,
            LabelPrinter = LabelPrinter, ReportPrinter = ReportPrinter, AutoPrint = AutoPrint, Copies = Copies,
            PaperWidth = PaperWidth, Orientation = Orientation, MarginMm = MarginMm, CutPaper = CutPaper, OpenDrawer = OpenDrawer
        }
    };

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new RMS.Modules.Settings.Application.UpdatePrinterSettings.UpdatePrinterSettingsCommand(BuildModel().Printer));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save printer settings."); return; }
        _snapshot = BuildModel(); IsDirty = false;
        ShowSuccess("Printer settings saved.");
    }

    public override async Task RestoreDefaultsAsync()
    {
        var result = await Mediator.Send(new RMS.Modules.Settings.Application.ResetSettings.ResetSettingsCommand());
        if (result.IsFailure) { ShowError(result.Error ?? "Could not reset settings."); return; }
        Load(result.Value);
        RequestGlobalReload?.Invoke();
        ShowSuccess("Settings restored to defaults.");
    }
}
