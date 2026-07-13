using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Domain;
using RMS.WPF.Commands;
using RMS.WPF.Printing;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class PrintingToolsViewModel : ViewModelBase
{
    private readonly IPrintingService _printing;
    private readonly IBarcodeGenerator _barcodes;
    private readonly IDialogService _dialogService;
    private string _status = "Ready";
    private bool _isBusy;

    public PrintingToolsViewModel(IPrintingService printing, IBarcodeGenerator barcodes, IDialogService dialogService)
    {
        _printing = printing;
        _barcodes = barcodes;
        _dialogService = dialogService;

        TestReceiptCommand = new RelayCommand(_ => _ = TestReceiptAsync());
        TestInvoiceCommand = new RelayCommand(_ => _ = TestInvoiceAsync());
        TestLabelCommand = new RelayCommand(_ => _ = TestLabelAsync());
        TestBarcodeCommand = new RelayCommand(_ => _ = TestBarcodeAsync());
        OpenDesignerCommand = new RelayCommand(_ => OpenDesigner());
        SelectPrinterCommand = new RelayCommand(_ => _ = SelectPrinterAsync());
        RefreshPrintersCommand = new RelayCommand(_ => _ = RefreshPrintersAsync());
    }

    public ObservableCollection<string> Printers { get; } = new();

    public string Status { get => _status; private set { _status = value; OnPropertyChanged(); } }
    public bool IsBusy { get => _isBusy; private set { _isBusy = value; OnPropertyChanged(); } }

    public ICommand TestReceiptCommand { get; }
    public ICommand TestInvoiceCommand { get; }
    public ICommand TestLabelCommand { get; }
    public ICommand TestBarcodeCommand { get; }
    public ICommand OpenDesignerCommand { get; }
    public ICommand SelectPrinterCommand { get; }
    public ICommand RefreshPrintersCommand { get; }

    private async Task RefreshPrintersAsync()
    {
        try
        {
            var printers = await _printing.GetPrintersAsync();
            Printers.Clear();
            foreach (var p in printers) Printers.Add(p.Name);
            Status = $"{printers.Count} printer(s) detected.";
        }
        catch (Exception ex)
        {
            Status = $"Could not list printers: {ex.Message}";
        }
    }

    private async Task TestReceiptAsync() => await RunTest("receipt", () => _printing.PrintTestReceiptAsync());
    private async Task TestInvoiceAsync() => await RunTest("invoice", () => _printing.PrintTestInvoiceAsync());
    private async Task TestLabelAsync() => await RunTest("label", () => _printing.PrintTestLabelAsync());
    private async Task TestBarcodeAsync() => await RunTest("barcode", () => _printing.PrintTestBarcodeAsync());

    private async Task RunTest(string kind, Func<Task<RMS.BuildingBlocks.Results.Result<string>>> action)
    {
        IsBusy = true;
        Status = $"Printing test {kind}...";
        try
        {
            var result = await action();
            if (result.IsSuccess)
            {
                Status = $"Test {kind} printed. Preview opened.";
                OpenPreview(result.Value);
            }
            else
            {
                Status = result.Error ?? "Print failed.";
                _dialogService.ShowError(result.Error ?? "Print failed.");
            }
        }
        catch (Exception ex)
        {
            Status = ex.Message;
            _dialogService.ShowError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenPreview(string pdfPath)
    {
        var window = new PrintPreviewWindow(pdfPath) { Owner = Application.Current.MainWindow };
        window.Show();
    }

    private void OpenDesigner()
    {
        var window = new BarcodeLabelDesignerWindow(_barcodes, _printing, _dialogService) { Owner = Application.Current.MainWindow };
        window.Show();
    }

    private async Task SelectPrinterAsync()
    {
        var printers = await _printing.GetPrintersAsync();
        var dialog = new PrinterSelectionDialog { Owner = Application.Current.MainWindow };
        dialog.LoadPrinters(printers.Select(p => p.Name), null);
        if (dialog.ShowDialog() == true)
        {
            var name = dialog.SelectedPrinter;
            var status = name is null or "" ? "No printer selected." : (await _printing.GetPrinterStatusAsync(name)).Value.ToString();
            _dialogService.ShowInfo($"Selected printer: {name}\nStatus: {status}");
        }
    }
}
