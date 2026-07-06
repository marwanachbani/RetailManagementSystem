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
        private readonly IServiceProvider _serviceProvider;
        private string _searchText = string.Empty;
        private string? _statusMessage;
        private bool _showInactive;
        private bool _isLoading;
        private bool _hasData;
        private ProductReadModel? _selectedProduct;
        private int _pageNumber = 1;
        private int _totalPages = 1;
        public int PageSize { get; } = 25;

        public ProductListViewModel(IMediator mediator, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _serviceProvider = serviceProvider;
            SearchCommand = new RelayCommand(_ => _ = LoadProductsAsync());
            RefreshCommand = new RelayCommand(_ => _ = LoadProductsAsync());
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());
            CreateProductCommand = new RelayCommand(_ => _ = CreateProductAsync());
            AddCommand = new RelayCommand(_ => _ = CreateProductAsync());
            EditProductCommand = new RelayCommand(o => _ = EditProductAsync((Guid)o!), _ => SelectedProduct is not null);
            EditCommand = new RelayCommand(_ => _ = EditProductFromSelection(), _ => SelectedProduct is not null);
            DeactivateCommand = new RelayCommand(_ => _ = DeactivateProductAsync(), _ => SelectedProduct?.IsActive == true);
            PreviousPageCommand = new RelayCommand(_ => _ = PreviousPageAsync(), _ => PageNumber > 1);
            NextPageCommand = new RelayCommand(_ => _ = NextPageAsync(), _ => PageNumber < TotalPages);
            _ = LoadProductsAsync();
        }

    public ObservableCollection<ProductReadModel> Products { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
    }

    public string SearchTerm
    {
        get => SearchText;
        set => SearchText = value;
    }

    public ProductReadModel? SelectedProduct
    {
        get => _selectedProduct;
        set { _selectedProduct = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool ShowInactive
    {
        get => _showInactive;
        set { _showInactive = value; OnPropertyChanged(); _ = LoadProductsAsync(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool HasData
    {
        get => _hasData;
        private set { _hasData = value; OnPropertyChanged(); }
    }

    public int PageNumber
    {
        get => _pageNumber;
        private set { _pageNumber = value; OnPropertyChanged(); }
    }

    public int TotalPages
    {
        get => _totalPages;
        private set { _totalPages = value; OnPropertyChanged(); }
    }

    public ICommand SearchCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand CreateProductCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditProductCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeactivateCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }

    public async Task LoadProductsAsync()
    {
        IsLoading = true;
        try
        {
            var query = new GetProductsPagedQuery(
                PageNumber: PageNumber,
                PageSize: PageSize,
                SearchTerm: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
                IncludeInactive: ShowInactive);
            var result = await _mediator.Send(query);
            Products.Clear();
            if (result.IsSuccess)
            {
                foreach (var item in result.Value.Items)
                    Products.Add(item);
                TotalPages = Math.Max(1, result.Value.TotalPages);
                StatusMessage = $"{result.Value.TotalCount} products";
            }
            else
            {
                StatusMessage = result.Error;
            }
            HasData = Products.Count > 0;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error loading products");
            Products.Clear();
            HasData = false;
        }
        finally
        {
            IsLoading = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void ClearFilter()
    {
        SearchText = string.Empty;
        ShowInactive = false;
        PageNumber = 1;
        _ = LoadProductsAsync();
    }

    private async Task CreateProductAsync()
    {
        var dialog = (CreateProductWindow)_serviceProvider.GetService(typeof(CreateProductWindow))!;
        if (dialog.ShowDialog() == true)
        {
            await LoadProductsAsync();
        }
    }

    private async Task EditProductAsync(Guid id)
    {
        var dialog = (EditProductWindow)_serviceProvider.GetService(typeof(EditProductWindow))!;
        dialog.LoadProduct(id);
        if (dialog.ShowDialog() == true)
        {
            await LoadProductsAsync();
        }
    }

    private async Task EditProductFromSelection()
    {
        if (SelectedProduct is not null)
            await EditProductAsync(SelectedProduct.Id);
    }

    private async Task DeactivateProductAsync()
    {
        if (SelectedProduct is null) return;
        var result = MessageBox.Show(
            $"Are you sure you want to deactivate {SelectedProduct.Name}?",
            "Confirm Deactivation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        var commandResult = await _mediator.Send(new DeactivateProductCommand(SelectedProduct.Id));
        if (commandResult.IsSuccess)
        {
            StatusMessage = $"{SelectedProduct.Name} deactivated.";
            await LoadProductsAsync();
        }
        else
        {
            StatusMessage = commandResult.Error;
        }
    }

    private async Task PreviousPageAsync()
    {
        if (PageNumber > 1)
        {
            PageNumber--;
            await LoadProductsAsync();
        }
    }

    private async Task NextPageAsync()
    {
        if (PageNumber < TotalPages)
        {
            PageNumber++;
            await LoadProductsAsync();
        }
    }
}
