using MediatR;
using RMS.Modules.Reporting.Application.Contracts;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class ReportsViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;

    public SalesReportViewModel SalesViewModel { get; }
    public InventoryReportViewModel InventoryViewModel { get; }
    public PurchasingReportViewModel PurchasingViewModel { get; }
    public CustomerReportViewModel CustomerViewModel { get; }
    public SupplierReportViewModel SupplierViewModel { get; }
    public ProductReportViewModel ProductViewModel { get; }
    public FinancialReportViewModel FinancialViewModel { get; }

    public ReportsViewModel(
        IServiceProvider serviceProvider,
        SalesReportViewModel salesViewModel,
        InventoryReportViewModel inventoryViewModel,
        PurchasingReportViewModel purchasingViewModel,
        CustomerReportViewModel customerViewModel,
        SupplierReportViewModel supplierViewModel,
        ProductReportViewModel productViewModel,
        FinancialReportViewModel financialViewModel)
    {
        _serviceProvider = serviceProvider;

        SalesViewModel = salesViewModel;
        InventoryViewModel = inventoryViewModel;
        PurchasingViewModel = purchasingViewModel;
        CustomerViewModel = customerViewModel;
        SupplierViewModel = supplierViewModel;
        ProductViewModel = productViewModel;
        FinancialViewModel = financialViewModel;

        _ = SalesViewModel.LoadAsync();
    }
}
