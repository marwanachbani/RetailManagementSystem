using System.Windows.Input;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private object? _currentViewModel;

    public MainWindowViewModel(ProductListViewModel productListViewModel)
    {
        ProductListViewModel = productListViewModel;
        NavigateProductsCommand = new RelayCommand(_ => CurrentViewModel = ProductListViewModel);
        CurrentViewModel = ProductListViewModel;
    }

    public ProductListViewModel ProductListViewModel { get; }

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
}
