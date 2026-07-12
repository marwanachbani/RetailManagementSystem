using System.Collections.ObjectModel;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.Modules.Reporting.Application.IntegrationEvents;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class PurchasingReportViewModel : ReportViewModelBase
{
    private readonly ObservableCollection<PurchaseReportItem> _purchaseItems = new();
    private readonly ObservableCollection<PurchaseByProductItem> _productItems = new();
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private string? _searchTerm;
    private Guid? _supplierId;
    private string _selectedReport = "Purchase Orders";

    public override ObservableCollection<object> Items => _selectedReport == "Purchase Orders" ? new ObservableCollection<object>(_purchaseItems) : new ObservableCollection<object>(_productItems);

    public ObservableCollection<PurchaseReportItem> TypedPurchaseItems => _purchaseItems;
    public ObservableCollection<PurchaseByProductItem> TypedProductItems => _productItems;

    public string SelectedReport
    {
        get => _selectedReport;
        set { _selectedReport = value; OnPropertyChanged(); OnPropertyChanged(nameof(Items)); _ = LoadAsync(); }
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

    public string? SearchTerm
    {
        get => _searchTerm;
        set { _searchTerm = value; OnPropertyChanged(); }
    }

    public Guid? SupplierId
    {
        get => _supplierId;
        set { _supplierId = value; OnPropertyChanged(); }
    }

    public PurchasingReportViewModel(IReportingReadStore readStore, IDialogService dialogService, IEventBus eventBus)
        : base(readStore, dialogService, eventBus)
    {
    }

    public override async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = $"Loading {SelectedReport}...";

        try
        {
            if (SelectedReport == "Purchase Orders")
            {
                var result = await ReadStore.GetPurchaseReportAsync(new DateRangeFilter(FromDate, ToDate), SupplierId, SearchTerm, null, false);
                _purchaseItems.Clear();
                foreach (var item in result.Items)
                    _purchaseItems.Add(item);
                StatusMessage = $"Loaded {result.TotalCount} purchase orders. Total: {result.GrandTotalCost:C}";
            }
            else
            {
                var result = await ReadStore.GetPurchaseByProductAsync(new DateRangeFilter(FromDate, ToDate), SearchTerm);
                _productItems.Clear();
                foreach (var item in result.Items)
                    _productItems.Add(item);
                StatusMessage = $"Loaded {result.TotalCount} products. Total: {result.GrandTotalCost:C}";
            }

            await EventBus.PublishAsync(new ReportGeneratedIntegrationEvent("Purchasing", "View"), default);
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
