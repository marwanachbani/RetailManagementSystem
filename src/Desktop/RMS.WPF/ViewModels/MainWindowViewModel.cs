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
        SalesViewModel salesViewModel)
    {
        DashboardViewModel = dashboardViewModel;
        ProductListViewModel = productListViewModel;
        InventoryListViewModel = inventoryListViewModel;
        SalesViewModel = salesViewModel;
        NavigateDashboardCommand = new RelayCommand(_ => CurrentViewModel = DashboardViewModel);
        NavigateProductsCommand = new RelayCommand(_ => CurrentViewModel = ProductListViewModel);
        NavigateInventoryCommand = new RelayCommand(_ => CurrentViewModel = InventoryListViewModel);
        NavigateSalesCommand = new RelayCommand(_ => CurrentViewModel = SalesViewModel);
        LogoutCommand = new RelayCommand(_ => Logout());
        CurrentViewModel = DashboardViewModel;
    }

    public DashboardViewModel DashboardViewModel { get; }
    public ProductListViewModel ProductListViewModel { get; }
    public InventoryListViewModel InventoryListViewModel { get; }
    public SalesViewModel SalesViewModel { get; }

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
    public ICommand LogoutCommand { get; }

    public event EventHandler? RequestLogout;

    private void Logout()
    {
        RequestLogout?.Invoke(this, EventArgs.Empty);
    }
}
