using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Application.SearchProducts;
using RMS.Modules.Sales.Application.AddSaleItem;
using RMS.Modules.Sales.Application.CompleteSale;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Application.CreateSale;
using RMS.Modules.Sales.Application.GetSaleById;
using RMS.Modules.Sales.Application.RemoveSaleItem;
using RMS.Modules.Sales.Domain.Entities;
using RMS.Modules.Sales.Infrastructure.ReceiptGeneration;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.ViewModels;

public sealed class CreateSaleViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private readonly IReceiptGenerator _receiptGenerator;
    private readonly ICurrentSessionService _session;
    private Guid _saleId;
    private Guid _cashierId;
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
    private int _productRequestSequence;
    private string? _lastReceiptPath;
    private bool _isSaleCompleted;

    public CreateSaleViewModel(IMediator mediator, IDialogService dialogService, IReceiptGenerator receiptGenerator, ICurrentSessionService session)
    {
        _mediator = mediator;
        _dialogService = dialogService;
        _receiptGenerator = receiptGenerator;
        _session = session;
        SearchProductsCommand = new RelayCommand(_ => _ = LoadProductsAsync());
        ScanBarcodeCommand = new RelayCommand(_ => _ = ScanBarcodeAsync());
        AddToCartCommand = new RelayCommand(o => _ = AddToCartAsync((ProductReadModel)o!, 1));
        AddSelectedToCartCommand = new RelayCommand(_ => _ = AddSelectedToCartAsync(), _ => SelectedProduct is not null);
        IncreaseQuantityCommand = new RelayCommand(o => _ = ChangeQuantityAsync((SaleItemReadModel)o!, 1));
        DecreaseQuantityCommand = new RelayCommand(o => _ = ChangeQuantityAsync((SaleItemReadModel)o!, -1));
        RemoveFromCartCommand = new RelayCommand(o => _ = RemoveFromCartAsync((SaleItemReadModel)o!));
        CompleteSaleCommand = new RelayCommand(_ => _ = CompleteSaleAsync(), _ => Items.Count > 0 && !IsSaleCompleted);
        PrintReceiptCommand = new RelayCommand(_ => PrintReceipt(), _ => _lastReceiptPath is not null);
        StartNewSaleCommand = new RelayCommand(_ => _ = StartNewSaleAsync());
        CancelCommand = new RelayCommand(_ => CloseWithResult(IsSaleCompleted));
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
            _ = LoadProductsAsync();
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
        private set { _subTotal = value; OnPropertyChanged(); }
    }

    public decimal DiscountAmount
    {
        get => _discountAmount;
        private set { _discountAmount = value; OnPropertyChanged(); }
    }

    public decimal TaxAmount
    {
        get => _taxAmount;
        private set { _taxAmount = value; OnPropertyChanged(); }
    }

    public decimal TotalAmount
    {
        get => _totalAmount;
        private set { _totalAmount = value; OnPropertyChanged(); }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    /// <summary>True once the current sale has been completed — the cart is shown
    /// read-only and the cashier can print the receipt or start a fresh sale.</summary>
    public bool IsSaleCompleted
    {
        get => _isSaleCompleted;
        private set { _isSaleCompleted = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
    }

    public ICommand SearchProductsCommand { get; }
    public ICommand ScanBarcodeCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand AddSelectedToCartCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand CompleteSaleCommand { get; }
    public ICommand PrintReceiptCommand { get; }
    public ICommand StartNewSaleCommand { get; }
    public ICommand CancelCommand { get; }

    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;

    public async Task InitializeSaleAsync(Guid cashierId)
    {
        _cashierId = cashierId;
        var result = await _mediator.Send(new CreateSaleCommand(cashierId));
        if (result.IsSuccess)
        {
            _saleId = result.Value;
            IsSaleCompleted = false;
            await RefreshSaleAsync();
        }
        else
        {
            StatusMessage = result.Error;
            _dialogService.ShowError(result.Error ?? "Could not start a new sale.");
        }
    }

    /// <summary>Resume an existing pending sale instead of creating a brand new one.</summary>
    public async Task ResumeSaleAsync(Guid saleId, Guid cashierId)
    {
        _saleId = saleId;
        _cashierId = cashierId;
        IsSaleCompleted = false;
        await RefreshSaleAsync();
    }

    /// <summary>Start a brand new empty sale in the same window, after completing one —
    /// so the cashier can serve the next customer without closing and reopening the dialog.</summary>
    private async Task StartNewSaleAsync()
    {
        _lastReceiptPath = null;
        DiscountPercentage = 0;
        TaxPercentage = 0;
        SearchText = string.Empty;
        await InitializeSaleAsync(_cashierId);
    }

    public async Task LoadProductsAsync()
    {
        var requestId = ++_productRequestSequence;
        try
        {
            var query = new SearchProductsQuery(
                SearchTerm: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
                IncludeInactive: false);
            var result = await _mediator.Send(query);

            // A newer request already started (user kept typing) — discard this stale response.
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

    /// <summary>Called when the cashier presses Enter in the search box — the classic
    /// barcode-scanner workflow (scan sends the code followed by Enter). If the text
    /// exactly matches one product's barcode, add it straight to the cart.</summary>
    public async Task ScanBarcodeAsync()
    {
        var code = SearchText.Trim();
        if (code.Length == 0) return;

        var match = Products.FirstOrDefault(p => string.Equals(p.Barcode, code, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            // Not in the currently-filtered list — ask the backend directly by barcode.
            var result = await _mediator.Send(new SearchProductsQuery(code, false));
            match = result.IsSuccess
                ? result.Value.FirstOrDefault(p => string.Equals(p.Barcode, code, StringComparison.OrdinalIgnoreCase))
                : null;
        }

        if (match is null)
        {
            _dialogService.ShowWarning($"No product found with barcode \"{code}\".");
            return;
        }

        await AddToCartAsync(match, 1);
        SearchText = string.Empty;
    }

    private async Task AddSelectedToCartAsync()
    {
        if (SelectedProduct is null) return;
        await AddToCartAsync(SelectedProduct, 1);
    }

    private async Task AddToCartAsync(ProductReadModel product, int quantity)
    {
        if (_saleId == Guid.Empty || IsSaleCompleted)
        {
            StatusMessage = "Sale not initialized.";
            return;
        }

        var result = await _mediator.Send(new AddSaleItemCommand(
            _saleId, product.Id, product.Name, quantity, product.SalePrice));

        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            _dialogService.ShowError(result.Error ?? "Could not add the product to the cart.");
            return;
        }

        await RefreshSaleAsync();
        StatusMessage = null;
    }

    /// <summary>Adjust an existing cart line's quantity by +1/-1. There's no direct
    /// "set quantity" operation on the sale, so a decrease is done by removing the
    /// line and re-adding it with the lower quantity (matches the same domain rules
    /// as adding fresh).</summary>
    private async Task ChangeQuantityAsync(SaleItemReadModel item, int delta)
    {
        if (_saleId == Guid.Empty || IsSaleCompleted) return;

        if (delta > 0)
        {
            var result = await _mediator.Send(new AddSaleItemCommand(_saleId, item.ProductId, item.ProductName, delta, item.UnitPrice));
            if (result.IsFailure)
            {
                _dialogService.ShowError(result.Error ?? "Could not update the quantity.");
                return;
            }
        }
        else
        {
            var newQuantity = item.Quantity + delta;
            var removeResult = await _mediator.Send(new RemoveSaleItemCommand(_saleId, item.Id));
            if (removeResult.IsFailure)
            {
                _dialogService.ShowError(removeResult.Error ?? "Could not update the quantity.");
                return;
            }

            if (newQuantity > 0)
            {
                var addResult = await _mediator.Send(new AddSaleItemCommand(_saleId, item.ProductId, item.ProductName, newQuantity, item.UnitPrice));
                if (addResult.IsFailure)
                {
                    _dialogService.ShowError(addResult.Error ?? "Could not update the quantity.");
                    return;
                }
            }
        }

        await RefreshSaleAsync();
    }

    private async Task RemoveFromCartAsync(SaleItemReadModel item)
    {
        if (_saleId == Guid.Empty || IsSaleCompleted) return;

        if (!_dialogService.Confirm($"Remove \"{item.ProductName}\" from the cart?"))
            return;

        var result = await _mediator.Send(new RemoveSaleItemCommand(_saleId, item.Id));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            _dialogService.ShowError(result.Error ?? "Could not remove the item.");
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
        if (Items.Count == 0)
        {
            _dialogService.ShowWarning("Add at least one product to the cart before completing the sale.");
            return;
        }

        var result = await _mediator.Send(new CompleteSaleCommand(_saleId, DiscountPercentage, TaxPercentage));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            _dialogService.ShowError(result.Error ?? "Could not complete the sale.");
            return;
        }

        await RefreshSaleAsync();
        IsSaleCompleted = true;
        await GenerateReceiptAsync();
    }

    private async Task GenerateReceiptAsync()
    {
        try
        {
            var saleResult = await _mediator.Send(new GetSaleByIdQuery(_saleId));
            if (saleResult.IsFailure) return;

            var model = saleResult.Value;
            var sale = Sale.Rehydrate(
                model.Id, model.SaleNumber, model.CashierId, model.SaleDate,
                Enum.Parse<SaleStatus>(model.Status), model.SubTotal, model.DiscountAmount,
                model.TaxAmount, model.TotalAmount, model.DiscountPercentage, model.TaxPercentage,
                model.CompletedAt, model.RefundedAt, model.CreatedAt, model.Notes);
            var items = model.Items.Select(i => SaleItem.Create(i.Id, model.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice));
            sale.RehydrateItems(items);

            var outputDirectory = System.IO.Path.Combine(App.ProgramDataDirectory, "Receipts");
            var cashierName = _session.IsAuthenticated ? _session.FullName : null;
            _lastReceiptPath = await _receiptGenerator.GenerateReceiptAsync(sale, "Retail Management System", cashierName, outputDirectory);
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            // The sale itself already completed successfully — a receipt file failure
            // shouldn't block the cashier, just surface it quietly.
            StatusMessage = $"Sale completed, but the receipt could not be generated: {ex.Message}";
        }
    }

    private void PrintReceipt()
    {
        if (_lastReceiptPath is null) return;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_lastReceiptPath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Could not open the receipt: {ex.Message}");
        }
    }

    private void CloseWithResult(bool success)
    {
        DialogResult = success;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
