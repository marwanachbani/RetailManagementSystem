using System.Windows.Input;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private object? _currentViewModel;

    public MainWindowViewModel(ProductListViewModel productListViewModel, InventoryListViewModel inventoryListViewModel, SalesViewModel salesViewModel)
    {
        ProductListViewModel = productListViewModel;
        InventoryListViewModel = inventoryListViewModel;
        SalesViewModel = salesViewModel;
        NavigateProductsCommand = new RelayCommand(_ => CurrentViewModel = ProductListViewModel);
        NavigateInventoryCommand = new RelayCommand(_ => CurrentViewModel = InventoryListViewModel);
        NavigateSalesCommand = new RelayCommand(_ => CurrentViewModel = SalesViewModel);
        CurrentViewModel = ProductListViewModel;
    }

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

    public ICommand NavigateProductsCommand { get; }
    public ICommand NavigateInventoryCommand { get; }
    public ICommand NavigateSalesCommand { get; }
}
