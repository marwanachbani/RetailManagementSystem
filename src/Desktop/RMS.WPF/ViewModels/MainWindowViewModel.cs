using System.Windows.Input;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private object? _currentViewModel;

    public MainWindowViewModel(
        DashboardViewModel dashboardViewModel,
        ProductListViewModel productListViewModel,
        InventoryListViewModel inventoryListViewModel)
    {
        DashboardViewModel = dashboardViewModel;
        ProductListViewModel = productListViewModel;
        InventoryListViewModel = inventoryListViewModel;
        NavigateDashboardCommand = new RelayCommand(_ => CurrentViewModel = DashboardViewModel);
        NavigateProductsCommand = new RelayCommand(_ => CurrentViewModel = ProductListViewModel);
        NavigateInventoryCommand = new RelayCommand(_ => CurrentViewModel = InventoryListViewModel);
        LogoutCommand = new RelayCommand(_ => _ = LogoutAsync());
        CurrentViewModel = DashboardViewModel;
    }

    public DashboardViewModel DashboardViewModel { get; }
    public ProductListViewModel ProductListViewModel { get; }
    public InventoryListViewModel InventoryListViewModel { get; }

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
    public ICommand LogoutCommand { get; }

    private Task LogoutAsync()
    {
        // In a real app, this would show a confirmation dialog and restart to login
        // For now, just navigate back to dashboard as a safe default
        CurrentViewModel = DashboardViewModel;
        return Task.CompletedTask;
    }
}
