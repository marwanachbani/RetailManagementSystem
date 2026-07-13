using System.Windows;
using System.Windows.Input;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Identity.Application.IntegrationEvents;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.WPF.Backup;
using RMS.WPF.Commands;
using RMS.WPF.Notifications;
using RMS.WPF.Services;
using RMS.WPF.Settings;

namespace RMS.WPF.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;
    private readonly ICurrentSessionService _currentSessionService;
    private readonly INotificationRepository _notificationRepository;
    private object? _currentViewModel;
    private string _currentViewTitle = "Dashboard";
    private string _currentBreadcrumb = "Home / Dashboard";
    private string _currentUserName = "Operator";
    private string _statusMessage = "Ready";
    private int _unreadNotificationCount;

    public MainWindowViewModel(
        DashboardViewModel dashboardViewModel,
        ProductListViewModel productListViewModel,
        InventoryListViewModel inventoryListViewModel,
        SalesViewModel salesViewModel,
        CustomerListViewModel customerListViewModel,
        SupplierListViewModel supplierListViewModel,
        PurchaseOrdersViewModel purchaseOrdersViewModel,
        ReportsViewModel reportsViewModel,
        SettingsViewModel settingsViewModel,
        AuditLogViewModel auditLogViewModel,
        BackupAndRestoreViewModel backupAndRestoreViewModel,
        NotificationCenterViewModel notificationCenterViewModel,
        PrintingToolsViewModel printingToolsViewModel,
        IEventBus eventBus,
        ICurrentSessionService currentSessionService,
        INotificationRepository notificationRepository)
    {
        _eventBus = eventBus;
        _currentSessionService = currentSessionService;
        _notificationRepository = notificationRepository;
        DashboardViewModel = dashboardViewModel;
        ProductListViewModel = productListViewModel;
        InventoryListViewModel = inventoryListViewModel;
        SalesViewModel = salesViewModel;
        CustomerListViewModel = customerListViewModel;
        SupplierListViewModel = supplierListViewModel;
        PurchaseOrdersViewModel = purchaseOrdersViewModel;
        ReportsViewModel = reportsViewModel;
        SettingsViewModel = settingsViewModel;
        AuditLogViewModel = auditLogViewModel;
        BackupAndRestoreViewModel = backupAndRestoreViewModel;
        NotificationCenterViewModel = notificationCenterViewModel;
        PrintingToolsViewModel = printingToolsViewModel;
        NavigateDashboardCommand = new RelayCommand(_ => ShowView(DashboardViewModel, "Dashboard", "Home / Dashboard"));
        NavigateProductsCommand = new RelayCommand(_ => ShowView(ProductListViewModel, "Products", "Home / Products"));
        NavigateInventoryCommand = new RelayCommand(_ => ShowView(InventoryListViewModel, "Inventory", "Home / Inventory"));
        NavigateSalesCommand = new RelayCommand(_ => ShowView(SalesViewModel, "Sales", "Home / Sales"));
        NavigateCustomersCommand = new RelayCommand(_ => ShowView(CustomerListViewModel, "Customers", "Home / Customers"));
        NavigateSuppliersCommand = new RelayCommand(_ => ShowView(SupplierListViewModel, "Suppliers", "Home / Suppliers"));
        NavigatePurchasingCommand = new RelayCommand(_ => ShowView(PurchaseOrdersViewModel, "Purchasing", "Home / Purchasing"));
        NavigateReportsCommand = new RelayCommand(_ => ShowView(ReportsViewModel, "Reports", "Home / Reports"));
        NavigateSettingsCommand = new RelayCommand(_ => ShowView(SettingsViewModel, "Settings", "Home / Settings"));
        NavigateAuditCommand = new RelayCommand(_ => ShowView(AuditLogViewModel, "Audit Log", "Home / Audit Log"));
        NavigateBackupCommand = new RelayCommand(_ => ShowView(BackupAndRestoreViewModel, "Backup & Restore", "Home / Administration / Backup & Restore"));
        NavigatePrintingCommand = new RelayCommand(_ => ShowView(PrintingToolsViewModel, "Printing", "Home / Printing"));
        NavigateNotificationsCommand = new RelayCommand(_ => OpenNotificationCenter());
        LogoutCommand = new RelayCommand(_ => Logout());

        WireDashboardQuickActions(dashboardViewModel);

        _ = RefreshUnreadCountAsync();

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
    public AuditLogViewModel AuditLogViewModel { get; }
    public BackupAndRestoreViewModel BackupAndRestoreViewModel { get; }
    public NotificationCenterViewModel NotificationCenterViewModel { get; }
    public PrintingToolsViewModel PrintingToolsViewModel { get; }

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

    public int UnreadNotificationCount
    {
        get => _unreadNotificationCount;
        private set
        {
            _unreadNotificationCount = value;
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
    public ICommand NavigateAuditCommand { get; }
    public ICommand NavigateBackupCommand { get; }
    public ICommand NavigatePrintingCommand { get; }
    public ICommand NavigateNotificationsCommand { get; }
    public ICommand LogoutCommand { get; }

    public event EventHandler? RequestLogout;

    private void ShowView(object viewModel, string title, string breadcrumb)
    {
        CurrentViewModel = viewModel;
        CurrentViewTitle = title;
        CurrentBreadcrumb = breadcrumb;
        StatusMessage = $"{title} ready";
    }

    private void OpenNotificationCenter()
    {
        var window = new NotificationCenterView
        {
            Owner = Application.Current.MainWindow,
            DataContext = NotificationCenterViewModel
        };
        window.ShowDialog();
        _ = RefreshUnreadCountAsync();
    }

    private async Task RefreshUnreadCountAsync()
    {
        try
        {
            UnreadNotificationCount = _currentSessionService.IsAuthenticated
                ? await _notificationRepository.GetUnreadCountByUserIdAsync(_currentSessionService.UserId)
                : await _notificationRepository.GetUnreadCountAsync();
        }
        catch
        {
            UnreadNotificationCount = 0;
        }
    }

    private void Logout()
    {
        var result = MessageBox.Show("Do you want to log out and return to the sign-in screen?", "Log out", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        _eventBus.PublishAsync(new LogoutSucceededIntegrationEvent(
            _currentSessionService.UserId,
            _currentSessionService.UserName), default).GetAwaiter().GetResult();

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
