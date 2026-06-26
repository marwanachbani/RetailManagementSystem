using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.AddSaleItem;
using RMS.Modules.Sales.Application.CompleteSale;
using RMS.Modules.Sales.Application.CreateSale;
using RMS.Modules.Sales.Application.RemoveSaleItem;
using RMS.Modules.Sales.Application.Contracts;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class CreateSaleViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private Guid _saleId;
    private string? _statusMessage;
    private string _productName = "";
    private int _quantity = 1;
    private decimal _unitPrice;
    private decimal _discountPercentage;
    private decimal _taxPercentage;

    public CreateSaleViewModel(IMediator mediator)
    {
        _mediator = mediator;
        AddItemCommand = new RelayCommand(_ => _ = AddItemAsync(), _ => CanAddItem);
        RemoveItemCommand = new RelayCommand(_ => _ = RemoveItemAsync(), _ => SelectedItem is not null);
        CompleteSaleCommand = new RelayCommand(_ => _ = CompleteSaleAsync(), _ => Items.Count > 0);
        CancelCommand = new RelayCommand(_ => CloseWithResult(false));
    }

    public ObservableCollection<SaleItemReadModel> Items { get; } = new();

    public SaleItemReadModel? SelectedItem { get; set; }

    public string ProductName
    {
        get => _productName;
        set
        {
            _productName = value;
            OnPropertyChanged();
        }
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged();
        }
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            _unitPrice = value;
            OnPropertyChanged();
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

    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand CompleteSaleCommand { get; }
    public ICommand CancelCommand { get; }

    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;

    private bool CanAddItem => !string.IsNullOrWhiteSpace(ProductName) && Quantity > 0 && UnitPrice >= 0;

    public async Task InitializeSaleAsync(Guid cashierId)
    {
        var result = await _mediator.Send(new CreateSaleCommand(cashierId));
        if (result.IsSuccess)
            _saleId = result.Value;
    }

    private async Task AddItemAsync()
    {
        var productId = Guid.NewGuid(); // In real app, this would come from product selection
        var result = await _mediator.Send(new AddSaleItemCommand(_saleId, productId, ProductName, Quantity, UnitPrice));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        // Reload sale to get updated items
        // For simplicity, just add to local collection
        Items.Add(new SaleItemReadModel(Guid.NewGuid(), productId, ProductName, Quantity, UnitPrice, Quantity * UnitPrice));
        RecalculateTotals();
        ProductName = "";
        Quantity = 1;
        UnitPrice = 0;
    }

    private async Task RemoveItemAsync()
    {
        if (SelectedItem is null) return;
        var result = await _mediator.Send(new RemoveSaleItemCommand(_saleId, SelectedItem.Id));
        if (result.IsSuccess)
        {
            Items.Remove(SelectedItem);
            RecalculateTotals();
        }
    }

    private async Task CompleteSaleAsync()
    {
        var result = await _mediator.Send(new CompleteSaleCommand(_saleId, DiscountPercentage, TaxPercentage));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }
        CloseWithResult(true);
    }

    private void RecalculateTotals()
    {
        SubTotal = Items.Sum(i => i.TotalPrice);
        DiscountAmount = SubTotal * (DiscountPercentage / 100);
        var afterDiscount = SubTotal - DiscountAmount;
        TaxAmount = afterDiscount * (TaxPercentage / 100);
        TotalAmount = afterDiscount + TaxAmount;
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(TotalAmount));
    }

    private void CloseWithResult(bool success)
    {
        DialogResult = success;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
