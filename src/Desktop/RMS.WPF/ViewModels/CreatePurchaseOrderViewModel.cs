using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Application.SearchProducts;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Application.CreatePurchaseOrder;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class CreatePurchaseOrderViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISupplierReadStore _supplierReadStore;
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

    public CreatePurchaseOrderViewModel(IMediator mediator, ISupplierReadStore supplierReadStore)
    {
        _mediator = mediator;
        _supplierReadStore = supplierReadStore;
        SearchProductsCommand = new RelayCommand(_ => _ = LoadProductsAsync());
        SearchSuppliersCommand = new RelayCommand(_ => _ = LoadSuppliersAsync());
        AddItemCommand = new RelayCommand(_ => _ = AddItemAsync(), _ => SelectedProduct is not null && Quantity > 0 && UnitCost > 0);
        RemoveItemCommand = new RelayCommand(o => _ = RemoveItemAsync((PurchaseOrderItemDto)o!), _ => SelectedItem is not null);
        SubmitCommand = new RelayCommand(_ => _ = SubmitAsync(), _ => Items.Count > 0 && SelectedSupplier is not null);
        CancelCommand = new RelayCommand(_ => CloseWithResult(false));
        _ = LoadProductsAsync();
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
        }
    }

    public string SupplierSearchText
    {
        get => _supplierSearchText;
        set
        {
            _supplierSearchText = value;
            OnPropertyChanged();
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
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public PurchaseOrderItemDto? SelectedItem { get; set; }

    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }

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

    private async Task LoadProductsAsync()
    {
        var query = new SearchProductsQuery(string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(), false);
        var result = await _mediator.Send(query);
        Products.Clear();
        if (result.IsSuccess)
        {
            foreach (var product in result.Value)
                Products.Add(product);
        }
    }

    private async Task LoadSuppliersAsync()
    {
        var suppliers = await _supplierReadStore.SearchAsync(SupplierSearchText, false);
        Suppliers.Clear();
        foreach (var supplier in suppliers)
            Suppliers.Add(supplier);
    }

    private Task AddItemAsync()
    {
        if (SelectedProduct is null) return Task.CompletedTask;
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
        if (SelectedSupplier is null) return;
        var command = new CreatePurchaseOrderCommand(
            SelectedSupplier.Id, SelectedSupplier.CompanyName, Notes, TaxPercentage,
            Items.Select(i => new CreatePurchaseOrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitCost)).ToList());
        var result = await _mediator.Send(command);
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
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
