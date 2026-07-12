using System.Collections.ObjectModel;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.Modules.Reporting.Application.IntegrationEvents;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class InventoryReportViewModel : ReportViewModelBase
{
    private readonly ObservableCollection<object> _currentInventoryItems = new();
    private readonly ObservableCollection<StockMovementItem> _stockMovementItems = new();
    private string _selectedReport = "Current Inventory";
    private string? _searchTerm;
    private DateTime? _fromDate;
    private DateTime? _toDate;

    public override ObservableCollection<object> Items => _selectedReport == "Current Inventory" ? _currentInventoryItems : new ObservableCollection<object>(_stockMovementItems);

    public string SelectedReport
    {
        get => _selectedReport;
        set { _selectedReport = value; OnPropertyChanged(); OnPropertyChanged(nameof(Items)); _ = LoadAsync(); }
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set { _searchTerm = value; OnPropertyChanged(); }
    }

    public DateTime? FromDate
    {
        get => _fromDate;
        set { _fromDate = value; OnPropertyChanged(); }
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set { _toDate = value; OnPropertyChanged(); }
    }

    public ObservableCollection<InventoryReportItem> TypedInventoryItems => new(_currentInventoryItems.OfType<InventoryReportItem>());

    public InventoryReportViewModel(IReportingReadStore readStore, IDialogService dialogService, IEventBus eventBus)
        : base(readStore, dialogService, eventBus)
    {
    }

    public override async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = $"Loading {SelectedReport}...";

        try
        {
            if (SelectedReport == "Current Inventory")
            {
                var result = await ReadStore.GetInventoryReportAsync(SearchTerm);
                _currentInventoryItems.Clear();
                foreach (var item in result.Items)
                    _currentInventoryItems.Add(item);
                StatusMessage = $"Loaded {result.TotalCount} products. Value: {result.TotalInventoryValue:C}";
            }
            else
            {
                var result = await ReadStore.GetStockMovementAsync(new DateRangeFilter(FromDate, ToDate), SearchTerm);
                _stockMovementItems.Clear();
                foreach (var item in result.Items)
                    _stockMovementItems.Add(item);
                StatusMessage = $"Loaded {result.TotalCount} transactions";
            }

            await EventBus.PublishAsync(new ReportGeneratedIntegrationEvent("Inventory", "View"), default);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading report: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(Items));
        }
    }
}
