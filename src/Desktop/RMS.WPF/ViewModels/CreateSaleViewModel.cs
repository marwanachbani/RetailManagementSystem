using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Application.SearchProducts;
using RMS.Modules.Sales.Application.AddSaleItem;
using RMS.Modules.Sales.Application.CompleteSale;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Application.CreateSale;
using RMS.Modules.Sales.Application.GetSaleById;
using RMS.Modules.Sales.Application.RemoveSaleItem;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class CreateSaleViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private Guid _saleId;
    private string? _statusMessage;
    private string _searchText = string.Empty;
    private ProductReadModel? _selectedProduct;
    private SaleItemReadModel? _selectedCartItem;
    private decimal _discountPercentage;
    private decimal _taxPercentage;
    private decimal _subTotal;
    private decimal _discountAmount;
    private decimal _taxAmount;
    private decimal _totalAmount;

    public CreateSaleViewModel(IMediator mediator)
    {
        _mediator = mediator;
        SearchProductsCommand = new RelayCommand(_ => _ = LoadProductsAsync());
        AddToCartCommand = new RelayCommand(o => _ = AddToCartAsync((ProductReadModel)o!));
        RemoveFromCartCommand = new RelayCommand(o => _ = RemoveFromCartAsync((SaleItemReadModel)o!));
        CompleteSaleCommand = new RelayCommand(_ => _ = CompleteSaleAsync(), _ => Items.Count > 0);
        CancelCommand = new RelayCommand(_ => CloseWithResult(false));
        _ = LoadProductsAsync();
    }

    public ObservableCollection<ProductReadModel> Products { get; } = new();
    public ObservableCollection<SaleItemReadModel> Items { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
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
        }
    }

    public SaleItemReadModel? SelectedCartItem
    {
        get => _selectedCartItem;
        set
        {
            _selectedCartItem = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public decimal DiscountPercentage
    {
        get => _discountPercentage;
        set
        {
            _discountPercentage = value;
            OnPropertyChanged();
            RecalculateTotals();
        }
    }

    public decimal TaxPercentage
    {
        get => _taxPercentage;
        set
        {
            _taxPercentage = value;
            OnPropertyChanged();
            RecalculateTotals();
        }
    }

    public decimal SubTotal
    {
        get => _subTotal;
        private set
        {
            _subTotal = value;
            OnPropertyChanged();
        }
    }

    public decimal DiscountAmount
    {
        get => _discountAmount;
        private set
        {
            _discountAmount = value;
            OnPropertyChanged();
        }
    }

    public decimal TaxAmount
    {
        get => _taxAmount;
        private set
        {
            _taxAmount = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalAmount
    {
        get => _totalAmount;
        private set
        {
            _totalAmount = value;
            OnPropertyChanged();
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

    public ICommand SearchProductsCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand CompleteSaleCommand { get; }
    public ICommand CancelCommand { get; }

    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;

    public async Task InitializeSaleAsync(Guid cashierId)
    {
        var result = await _mediator.Send(new CreateSaleCommand(cashierId));
        if (result.IsSuccess)
        {
            _saleId = result.Value;
            await RefreshSaleAsync();
        }
        else
        {
            StatusMessage = result.Error;
        }
    }

    private async Task LoadProductsAsync()
    {
        var query = new SearchProductsQuery(
            SearchTerm: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            IncludeInactive: false);
        var result = await _mediator.Send(query);
        Products.Clear();
        if (result.IsSuccess)
        {
            foreach (var product in result.Value)
                Products.Add(product);
        }
    }

    private async Task AddToCartAsync(ProductReadModel product)
    {
        if (_saleId == Guid.Empty)
        {
            StatusMessage = "Sale not initialized.";
            return;
        }

        var result = await _mediator.Send(new AddSaleItemCommand(
            _saleId, product.Id, product.Name, 1, product.SalePrice));

        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        await RefreshSaleAsync();
        StatusMessage = null;
    }

    private async Task RemoveFromCartAsync(SaleItemReadModel item)
    {
        if (_saleId == Guid.Empty) return;

        var result = await _mediator.Send(new RemoveSaleItemCommand(_saleId, item.Id));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        await RefreshSaleAsync();
        StatusMessage = null;
    }

    private async Task RefreshSaleAsync()
    {
        if (_saleId == Guid.Empty) return;

        var result = await _mediator.Send(new GetSaleByIdQuery(_saleId));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        Items.Clear();
        foreach (var item in result.Value.Items)
            Items.Add(item);

        SubTotal = result.Value.SubTotal;
        RecalculateTotals();
        CommandManager.InvalidateRequerySuggested();
    }

    private void RecalculateTotals()
    {
        DiscountAmount = SubTotal * (DiscountPercentage / 100m);
        var afterDiscount = SubTotal - DiscountAmount;
        TaxAmount = afterDiscount * (TaxPercentage / 100m);
        TotalAmount = afterDiscount + TaxAmount;
    }

    private async Task CompleteSaleAsync()
    {
        var result = await _mediator.Send(new CompleteSaleCommand(_saleId, DiscountPercentage, TaxPercentage));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        await RefreshSaleAsync();
        CloseWithResult(true);
    }

    private void CloseWithResult(bool success)
    {
        DialogResult = success;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
