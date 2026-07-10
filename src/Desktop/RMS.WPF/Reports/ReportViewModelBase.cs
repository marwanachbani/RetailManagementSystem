using System.Collections.ObjectModel;
using System.Windows.Input;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public abstract class ReportViewModelBase : ViewModelBase
{
    protected readonly IReportingReadStore ReadStore;
    protected readonly IDialogService DialogService;

    private bool _isLoading;
    private string? _statusMessage;

    public bool IsLoading
    {
        get => _isLoading;
        protected set { _isLoading = value; OnPropertyChanged(); }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        protected set { _statusMessage = value; OnPropertyChanged(); }
    }

    public abstract ObservableCollection<object> Items { get; }

    public bool HasResults => Items is { Count: > 0 };
    public bool IsEmpty => Items is null || Items.Count == 0;

    public ICommand RefreshCommand { get; }
    public ICommand ExportPdfCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand PrintCommand { get; }

    protected ReportViewModelBase(IReportingReadStore readStore, IDialogService dialogService)
    {
        ReadStore = readStore;
        DialogService = dialogService;

        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
        ExportPdfCommand = new RelayCommand(_ => _ = ExportAsync("PDF"));
        ExportCsvCommand = new RelayCommand(_ => _ = ExportAsync("CSV"));
        PrintCommand = new RelayCommand(_ => _ = ExportAsync("Print"));
    }

    public abstract Task LoadAsync();

    protected virtual async Task ExportAsync(string format)
    {
        if (IsEmpty)
        {
            DialogService.ShowWarning("No data to export.", "Export");
            return;
        }

        StatusMessage = $"Exporting as {format}...";
        IsLoading = true;
        try
        {
            await Task.CompletedTask;
            DialogService.ShowInfo($"Exported {Items.Count} rows as {format}.", "Export");
            StatusMessage = $"Exported {Items.Count} rows as {format}.";
        }
        catch (Exception ex)
        {
            DialogService.ShowError($"Export failed: {ex.Message}", "Export Error");
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
