using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Application.SearchProducts;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Application.CreatePurchaseOrder;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class CreatePurchaseOrderViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISupplierReadStore _supplierReadStore;
    private readonly IDialogService _dialogService;
    private string? _statusMessage;
    private string _searchText = string.Empty;
    private string _supplierSearchText = string.Empty;
    private SupplierReadModel? _selectedSupplier;
    private ProductReadModel? _selectedProduct;
    private decimal _taxPercentage;
    private decimal _subTotal;
    private decimal _taxAmount;
    private decimal _totalAmount;
    private string? _notes;
    private int _quantity = 1;
    private decimal _unitCost;
    private int _productRequestSequence;

    public CreatePurchaseOrderViewModel(IMediator mediator, ISupplierReadStore supplierReadStore, IDialogService dialogService)
    {
        _mediator = mediator;
        _supplierReadStore = supplierReadStore;
        _dialogService = dialogService;
        SearchProductsCommand = new RelayCommand(_ => _ = LoadProductsAsync());
        SearchSuppliersCommand = new RelayCommand(_ => _ = LoadSuppliersAsync());
        AddItemCommand = new RelayCommand(_ => _ = AddItemAsync());
        RemoveItemCommand = new RelayCommand(o => _ = RemoveItemAsync((PurchaseOrderItemDto)o!));
        SubmitCommand = new RelayCommand(_ => _ = SubmitAsync());
        CancelCommand = new RelayCommand(_ => CloseWithResult(false));
        _ = LoadProductsAsync();
        _ = LoadSuppliersAsync();
    }

    public ObservableCollection<ProductReadModel> Products { get; } = new();
    public ObservableCollection<SupplierReadModel> Suppliers { get; } = new();
    public ObservableCollection<PurchaseOrderItemDto> Items { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            _ = LoadProductsAsync();
        }
    }

    public string SupplierSearchText
    {
        get => _supplierSearchText;
        set
        {
            _supplierSearchText = value;
            OnPropertyChanged();
            _ = LoadSuppliersAsync();
        }
    }

    public SupplierReadModel? SelectedSupplier
    {
        get => _selectedSupplier;
        set
        {
            _selectedSupplier = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ProductReadModel? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged();
            if (_selectedProduct is not null)
                UnitCost = _selectedProduct.CostPrice;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public PurchaseOrderItemDto? SelectedItem { get; set; }

    public int Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
    }

    public decimal UnitCost
    {
        get => _unitCost;
        set { _unitCost = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
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

    public string? Notes
    {
        get => _notes;
        set
        {
            _notes = value;
            OnPropertyChanged();
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
    public ICommand SearchSuppliersCommand { get; }
    public ICommand AddItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }

    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;

    public async Task LoadProductsAsync()
    {
        var requestId = ++_productRequestSequence;
        try
        {
            var query = new SearchProductsQuery(string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(), false);
            var result = await _mediator.Send(query);

            if (requestId != _productRequestSequence) return;

            if (result.IsSuccess)
            {
                Products.Clear();
                foreach (var product in result.Value)
                    Products.Add(product);
            }
            else
            {
                StatusMessage = result.Error;
                _dialogService.ShowError(result.Error ?? "Could not load the product catalog.");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            _dialogService.ShowError($"Could not load products: {ex.Message}");
        }
    }

    public async Task LoadSuppliersAsync()
    {
        var suppliers = await _supplierReadStore.SearchAsync(SupplierSearchText, false);
        Suppliers.Clear();
        foreach (var supplier in suppliers)
            Suppliers.Add(supplier);
    }

    private Task AddItemAsync()
    {
        if (SelectedProduct is null)
        {
            _dialogService.ShowWarning("Select a product from the list first.");
            return Task.CompletedTask;
        }
        if (Quantity <= 0)
        {
            _dialogService.ShowWarning("Quantity must be greater than zero.");
            return Task.CompletedTask;
        }
        if (UnitCost <= 0)
        {
            _dialogService.ShowWarning("Unit cost must be greater than zero.");
            return Task.CompletedTask;
        }

        var existing = Items.FirstOrDefault(i => i.ProductId == SelectedProduct.Id);
        if (existing is not null)
        {
            existing.Quantity += Quantity;
        }
        else
        {
            Items.Add(new PurchaseOrderItemDto(SelectedProduct.Id, SelectedProduct.Name, Quantity, UnitCost));
        }
        RecalculateTotals();
        CommandManager.InvalidateRequerySuggested();
        return Task.CompletedTask;
    }

    private Task RemoveItemAsync(PurchaseOrderItemDto item)
    {
        if (!_dialogService.Confirm($"Remove \"{item.ProductName}\" from this order?"))
            return Task.CompletedTask;

        Items.Remove(item);
        RecalculateTotals();
        CommandManager.InvalidateRequerySuggested();
        return Task.CompletedTask;
    }

    private void RecalculateTotals()
    {
        SubTotal = Items.Sum(i => i.Quantity * i.UnitCost);
        TaxAmount = SubTotal * (TaxPercentage / 100m);
        TotalAmount = SubTotal + TaxAmount;
    }

    private async Task SubmitAsync()
    {
        if (SelectedSupplier is null)
        {
            _dialogService.ShowWarning("Select a supplier before submitting the order.");
            return;
        }
        if (Items.Count == 0)
        {
            _dialogService.ShowWarning("Add at least one product before submitting the order.");
            return;
        }

        var command = new CreatePurchaseOrderCommand(
            SelectedSupplier.Id, SelectedSupplier.CompanyName, Notes, TaxPercentage,
            Items.Select(i => new CreatePurchaseOrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitCost)).ToList());
        var result = await _mediator.Send(command);
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            _dialogService.ShowError(result.Error ?? "Could not create the purchase order.");
            return;
        }
        CloseWithResult(true);
    }

    private void CloseWithResult(bool success)
    {
        DialogResult = success;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}

public sealed class PurchaseOrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost => Quantity * UnitCost;

    public PurchaseOrderItemDto(Guid productId, string productName, int quantity, decimal unitCost)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitCost = unitCost;
    }
}
