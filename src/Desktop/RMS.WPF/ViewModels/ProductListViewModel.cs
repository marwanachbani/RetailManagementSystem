using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Application.DeactivateProduct;
using RMS.Modules.Products.Application.GetProductsPaged;
using RMS.WPF.Commands;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class ProductListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _services;
    private string? _searchTerm;
    private ProductReadModel? _selectedProduct;
    private string? _statusMessage;
    private int _pageNumber = 1;
    private int _totalPages = 1;

    public ProductListViewModel(IMediator mediator, IServiceProvider services)
    {
        _mediator = mediator;
        _services = services;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
        SearchCommand = new RelayCommand(_ => _ = SearchAsync());
        AddCommand = new RelayCommand(_ => OpenCreateDialog());
        EditCommand = new RelayCommand(_ => OpenEditDialog(), _ => SelectedProduct is not null);
        DeactivateCommand = new RelayCommand(_ => _ = DeactivateAsync(), _ => SelectedProduct is not null && SelectedProduct.IsActive);
        NextPageCommand = new RelayCommand(_ => _ = NextPageAsync(), _ => PageNumber < TotalPages);
        PreviousPageCommand = new RelayCommand(_ => _ = PreviousPageAsync(), _ => PageNumber > 1);
        _ = LoadAsync();
    }

    public ObservableCollection<ProductReadModel> Products { get; } = new();
    public int PageSize { get; } = 25;

    public string? SearchTerm
    {
        get => _searchTerm;
        set
        {
            _searchTerm = value;
            OnPropertyChanged();
        }
    }

    public ProductReadModel? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public int PageNumber
    {
        get => _pageNumber;
        private set
        {
            _pageNumber = value;
            OnPropertyChanged();
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            _totalPages = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeactivateCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    public async Task LoadAsync()
    {
        var result = await _mediator.Send(new GetProductsPagedQuery(PageNumber, PageSize, SearchTerm, false));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        Products.Clear();
        foreach (var product in result.Value.Items)
            Products.Add(product);

        TotalPages = Math.Max(1, result.Value.TotalPages);
        StatusMessage = $"{result.Value.TotalCount} products";
    }

    private async Task SearchAsync()
    {
        PageNumber = 1;
        await LoadAsync();
    }

    private async Task NextPageAsync()
    {
        PageNumber++;
        await LoadAsync();
    }

    private async Task PreviousPageAsync()
    {
        PageNumber--;
        await LoadAsync();
    }

    private void OpenCreateDialog()
    {
        var window = (CreateProductWindow)_services.GetService(typeof(CreateProductWindow))!;
        if (window.ShowDialog() == true)
            _ = LoadAsync();
    }

    private void OpenEditDialog()
    {
        if (SelectedProduct is null)
            return;

        var window = (EditProductWindow)_services.GetService(typeof(EditProductWindow))!;
        window.LoadProduct(SelectedProduct.Id);
        if (window.ShowDialog() == true)
            _ = LoadAsync();
    }

    private async Task DeactivateAsync()
    {
        if (SelectedProduct is null)
            return;

        var confirm = MessageBox.Show($"Deactivate {SelectedProduct.Name}?", "Deactivate product", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes)
            return;

        var result = await _mediator.Send(new DeactivateProductCommand(SelectedProduct.Id));
        StatusMessage = result.IsSuccess ? "Product deactivated." : result.Error;
        await LoadAsync();
    }
}
