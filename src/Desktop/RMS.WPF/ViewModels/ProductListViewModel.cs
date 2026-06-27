using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Application.GetProductsPaged;
using RMS.WPF.Commands;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class ProductListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private ObservableCollection<ProductReadModel> _products = new();
    private string _searchText = string.Empty;
    private bool _showInactive;
    private bool _isLoading;
    private bool _hasData;

    public ProductListViewModel(IMediator mediator, IServiceProvider serviceProvider)
    {
        _mediator = mediator;
        _serviceProvider = serviceProvider;
        SearchCommand = new RelayCommand(_ => _ = LoadProductsAsync());
        ClearFilterCommand = new RelayCommand(_ => ClearFilter());
        CreateProductCommand = new RelayCommand(_ => _ = CreateProductAsync());
        EditProductCommand = new RelayCommand(o => _ = EditProductAsync((Guid)o!));
        _ = LoadProductsAsync();
    }

    public ObservableCollection<ProductReadModel> Products
    {
        get => _products;
        private set { _products = value; OnPropertyChanged(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
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

    public ICommand SearchCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand CreateProductCommand { get; }
    public ICommand EditProductCommand { get; }

    public async Task LoadProductsAsync()
    {
        IsLoading = true;
        try
        {
            var query = new GetProductsPagedQuery(
                PageNumber: 1,
                PageSize: 500,
                SearchTerm: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
                IncludeInactive: ShowInactive);
            var result = await _mediator.Send(query);
            Products = new ObservableCollection<ProductReadModel>(result.Value.Items);
            HasData = Products.Count > 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearFilter()
    {
        SearchText = string.Empty;
        ShowInactive = false;
        _ = LoadProductsAsync();
    }

    private async Task CreateProductAsync()
    {
        var dialog = _serviceProvider.GetRequiredService<CreateProductWindow>();
        if (dialog.ShowDialog() == true)
        {
            await LoadProductsAsync();
        }
    }

    private async Task EditProductAsync(Guid id)
    {
        var dialog = _serviceProvider.GetRequiredService<EditProductWindow>();
        dialog.LoadProduct(id);
        if (dialog.ShowDialog() == true)
        {
            await LoadProductsAsync();
        }
    }
}
