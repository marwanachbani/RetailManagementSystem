using System.Windows.Input;
using MediatR;
using RMS.WPF.Commands;
using RMS.WPF.Dashboard;
using RMS.WPF.Dashboard.Queries.GetDashboardSummary;
using RMS.WPF.Dashboard.Queries.GetRecentActivities;
using RMS.WPF.Dashboard.Queries.GetRecentPurchases;
using RMS.WPF.Dashboard.Queries.GetRecentSales;
using RMS.WPF.Dashboard.Queries.GetLowStockProducts;
using RMS.WPF.Dashboard.Queries.GetQuickStatistics;

namespace RMS.WPF.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private KpiSummary _kpiSummary = new();
    private QuickStatistics _quickStatistics = new();
    private string _statusMessage = "Loading...";
    private bool _isLoading = true;

    public DashboardViewModel(IMediator mediator)
    {
        _mediator = mediator;

        NewSaleCommand = new RelayCommand(_ => RequestNewSale?.Invoke(this, EventArgs.Empty));
        NewProductCommand = new RelayCommand(_ => RequestNewProduct?.Invoke(this, EventArgs.Empty));
        StockAdjustmentCommand = new RelayCommand(_ => RequestStockAdjustment?.Invoke(this, EventArgs.Empty));
        NewPurchaseOrderCommand = new RelayCommand(_ => RequestNewPurchaseOrder?.Invoke(this, EventArgs.Empty));
        NewCustomerCommand = new RelayCommand(_ => RequestNewCustomer?.Invoke(this, EventArgs.Empty));
        NewSupplierCommand = new RelayCommand(_ => RequestNewSupplier?.Invoke(this, EventArgs.Empty));
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());

        _ = LoadAsync();
    }

    public KpiSummary KpiSummary
    {
        get => _kpiSummary;
        private set { _kpiSummary = value; OnPropertyChanged(); }
    }

    public QuickStatistics QuickStatistics
    {
        get => _quickStatistics;
        private set { _quickStatistics = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    public List<RecentSaleDto> RecentSales { get; private set; } = new();
    public List<RecentPurchaseDto> RecentPurchases { get; private set; } = new();
    public List<LowStockProductDto> LowStockProducts { get; private set; } = new();
    public List<ActivityDto> RecentActivities { get; private set; } = new();
    public List<AlertDto> Alerts { get; private set; } = new();

    public ICommand NewSaleCommand { get; }
    public ICommand NewProductCommand { get; }
    public ICommand StockAdjustmentCommand { get; }
    public ICommand NewPurchaseOrderCommand { get; }
    public ICommand NewCustomerCommand { get; }
    public ICommand NewSupplierCommand { get; }
    public ICommand RefreshCommand { get; }

    public event EventHandler? RequestNewSale;
    public event EventHandler? RequestNewProduct;
    public event EventHandler? RequestStockAdjustment;
    public event EventHandler? RequestNewPurchaseOrder;
    public event EventHandler? RequestNewCustomer;
    public event EventHandler? RequestNewSupplier;

    public async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading dashboard...";
        try
        {
            var summaryResult = await _mediator.Send(new GetDashboardSummaryQuery());
            if (summaryResult.IsSuccess)
                KpiSummary = summaryResult.Value;

            var recentSalesResult = await _mediator.Send(new GetRecentSalesQuery(5));
            if (recentSalesResult.IsSuccess)
                RecentSales = recentSalesResult.Value.ToList();

            var recentPurchasesResult = await _mediator.Send(new GetRecentPurchasesQuery(5));
            if (recentPurchasesResult.IsSuccess)
                RecentPurchases = recentPurchasesResult.Value.ToList();

            var lowStockResult = await _mediator.Send(new GetLowStockProductsQuery(10));
            if (lowStockResult.IsSuccess)
                LowStockProducts = lowStockResult.Value.ToList();

            var activitiesResult = await _mediator.Send(new GetRecentActivitiesQuery(10));
            if (activitiesResult.IsSuccess)
                RecentActivities = activitiesResult.Value.ToList();

            var statsResult = await _mediator.Send(new GetQuickStatisticsQuery());
            if (statsResult.IsSuccess)
                QuickStatistics = statsResult.Value;

            BuildAlerts();

            OnPropertyChanged(nameof(RecentSales));
            OnPropertyChanged(nameof(RecentPurchases));
            OnPropertyChanged(nameof(LowStockProducts));
            OnPropertyChanged(nameof(RecentActivities));
            OnPropertyChanged(nameof(Alerts));

            StatusMessage = $"Dashboard updated at {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading dashboard: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildAlerts()
    {
        var alerts = new List<AlertDto>();

        if (KpiSummary.LowStockProducts > 0)
            alerts.Add(new AlertDto("Low Stock", $"{KpiSummary.LowStockProducts} products below threshold", "Warning", KpiSummary.LowStockProducts));

        if (KpiSummary.OutOfStockProducts > 0)
            alerts.Add(new AlertDto("Out of Stock", $"{KpiSummary.OutOfStockProducts} products unavailable", "Error", KpiSummary.OutOfStockProducts));

        if (KpiSummary.PurchaseOrdersToday > 0)
            alerts.Add(new AlertDto("Purchase Orders", $"{KpiSummary.PurchaseOrdersToday} orders placed today", "Info", KpiSummary.PurchaseOrdersToday));

        Alerts = alerts;
    }
}
