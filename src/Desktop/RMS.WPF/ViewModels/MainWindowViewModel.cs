using System.Windows.Input;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private object? _currentViewModel;

    public MainWindowViewModel(
        DashboardViewModel dashboardViewModel,
        ProductListViewModel productListViewModel,
        InventoryListViewModel inventoryListViewModel,
        SalesViewModel salesViewModel,
        CustomerListViewModel customerListViewModel,
        SupplierListViewModel supplierListViewModel,
        PurchaseOrdersViewModel purchaseOrdersViewModel)
    {
        DashboardViewModel = dashboardViewModel;
        ProductListViewModel = productListViewModel;
        InventoryListViewModel = inventoryListViewModel;
        SalesViewModel = salesViewModel;
        CustomerListViewModel = customerListViewModel;
        SupplierListViewModel = supplierListViewModel;
        PurchaseOrdersViewModel = purchaseOrdersViewModel;
        NavigateDashboardCommand = new RelayCommand(_ => CurrentViewModel = DashboardViewModel);
        NavigateProductsCommand = new RelayCommand(_ => CurrentViewModel = ProductListViewModel);
        NavigateInventoryCommand = new RelayCommand(_ => CurrentViewModel = InventoryListViewModel);
        NavigateSalesCommand = new RelayCommand(_ => CurrentViewModel = SalesViewModel);
        NavigateCustomersCommand = new RelayCommand(_ => CurrentViewModel = CustomerListViewModel);
        NavigateSuppliersCommand = new RelayCommand(_ => CurrentViewModel = SupplierListViewModel);
        NavigatePurchasingCommand = new RelayCommand(_ => CurrentViewModel = PurchaseOrdersViewModel);
        LogoutCommand = new RelayCommand(_ => Logout());
        CurrentViewModel = DashboardViewModel;
    }

    public DashboardViewModel DashboardViewModel { get; }
    public ProductListViewModel ProductListViewModel { get; }
    public InventoryListViewModel InventoryListViewModel { get; }
    public SalesViewModel SalesViewModel { get; }
    public CustomerListViewModel CustomerListViewModel { get; }
    public SupplierListViewModel SupplierListViewModel { get; }
    public PurchaseOrdersViewModel PurchaseOrdersViewModel { get; }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }

    public ICommand NavigateDashboardCommand { get; }
    public ICommand NavigateProductsCommand { get; }
    public ICommand NavigateInventoryCommand { get; }
    public ICommand NavigateSalesCommand { get; }
    public ICommand NavigateCustomersCommand { get; }
    public ICommand NavigateSuppliersCommand { get; }
    public ICommand NavigatePurchasingCommand { get; }
    public ICommand LogoutCommand { get; }

    public event EventHandler? RequestLogout;

    private void Logout()
    {
        RequestLogout?.Invoke(this, EventArgs.Empty);
    }
}
