using System.Windows;
using System.Windows.Input;
using RMS.WPF.Commands;
using RMS.WPF.Settings;

namespace RMS.WPF.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private object? _currentViewModel;
    private string _currentViewTitle = "Dashboard";
    private string _currentBreadcrumb = "Home / Dashboard";
    private string _currentUserName = "Operator";
    private string _statusMessage = "Ready";

    public MainWindowViewModel(
        DashboardViewModel dashboardViewModel,
        ProductListViewModel productListViewModel,
        InventoryListViewModel inventoryListViewModel,
        SalesViewModel salesViewModel,
        CustomerListViewModel customerListViewModel,
        SupplierListViewModel supplierListViewModel,
        PurchaseOrdersViewModel purchaseOrdersViewModel,
        ReportsViewModel reportsViewModel,
        SettingsViewModel settingsViewModel)
    {
        DashboardViewModel = dashboardViewModel;
        ProductListViewModel = productListViewModel;
        InventoryListViewModel = inventoryListViewModel;
        SalesViewModel = salesViewModel;
        CustomerListViewModel = customerListViewModel;
        SupplierListViewModel = supplierListViewModel;
        PurchaseOrdersViewModel = purchaseOrdersViewModel;
        ReportsViewModel = reportsViewModel;
        SettingsViewModel = settingsViewModel;
        NavigateDashboardCommand = new RelayCommand(_ => ShowView(DashboardViewModel, "Dashboard", "Home / Dashboard"));
        NavigateProductsCommand = new RelayCommand(_ => ShowView(ProductListViewModel, "Products", "Home / Products"));
        NavigateInventoryCommand = new RelayCommand(_ => ShowView(InventoryListViewModel, "Inventory", "Home / Inventory"));
        NavigateSalesCommand = new RelayCommand(_ => ShowView(SalesViewModel, "Sales", "Home / Sales"));
        NavigateCustomersCommand = new RelayCommand(_ => ShowView(CustomerListViewModel, "Customers", "Home / Customers"));
        NavigateSuppliersCommand = new RelayCommand(_ => ShowView(SupplierListViewModel, "Suppliers", "Home / Suppliers"));
        NavigatePurchasingCommand = new RelayCommand(_ => ShowView(PurchaseOrdersViewModel, "Purchasing", "Home / Purchasing"));
        NavigateReportsCommand = new RelayCommand(_ => ShowView(ReportsViewModel, "Reports", "Home / Reports"));
        NavigateSettingsCommand = new RelayCommand(_ => ShowView(SettingsViewModel, "Settings", "Home / Settings"));
        LogoutCommand = new RelayCommand(_ => Logout());

        WireDashboardQuickActions(dashboardViewModel);

        ShowView(DashboardViewModel, "Dashboard", "Home / Dashboard");
    }

    public DashboardViewModel DashboardViewModel { get; }
    public ProductListViewModel ProductListViewModel { get; }
    public InventoryListViewModel InventoryListViewModel { get; }
    public SalesViewModel SalesViewModel { get; }
    public CustomerListViewModel CustomerListViewModel { get; }
    public SupplierListViewModel SupplierListViewModel { get; }
    public PurchaseOrdersViewModel PurchaseOrdersViewModel { get; }
    public ReportsViewModel ReportsViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }

    public string CurrentViewTitle
    {
        get => _currentViewTitle;
        private set
        {
            _currentViewTitle = value;
            OnPropertyChanged();
        }
    }

    public string CurrentBreadcrumb
    {
        get => _currentBreadcrumb;
        private set
        {
            _currentBreadcrumb = value;
            OnPropertyChanged();
        }
    }

    public string CurrentUserName
    {
        get => _currentUserName;
        set
        {
            _currentUserName = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
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
    public ICommand NavigateReportsCommand { get; }
    public ICommand NavigateSettingsCommand { get; }
    public ICommand LogoutCommand { get; }

    public event EventHandler? RequestLogout;

    private void ShowView(object viewModel, string title, string breadcrumb)
    {
        CurrentViewModel = viewModel;
        CurrentViewTitle = title;
        CurrentBreadcrumb = breadcrumb;
        StatusMessage = $"{title} ready";
    }

    private void Logout()
    {
        var result = MessageBox.Show("Do you want to log out and return to the sign-in screen?", "Log out", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        RequestLogout?.Invoke(this, EventArgs.Empty);
    }

    private void WireDashboardQuickActions(DashboardViewModel dashboard)
    {
        dashboard.RequestNewSale += (_, _) => ShowView(SalesViewModel, "Sales", "Home / Sales");
        dashboard.RequestNewProduct += (_, _) => ShowView(ProductListViewModel, "Products", "Home / Products");
        dashboard.RequestStockAdjustment += (_, _) => ShowView(InventoryListViewModel, "Inventory", "Home / Inventory");
        dashboard.RequestNewPurchaseOrder += (_, _) => ShowView(PurchaseOrdersViewModel, "Purchasing", "Home / Purchasing");
        dashboard.RequestNewCustomer += (_, _) => ShowView(CustomerListViewModel, "Customers", "Home / Customers");
        dashboard.RequestNewSupplier += (_, _) => ShowView(SupplierListViewModel, "Suppliers", "Home / Suppliers");
    }
}
