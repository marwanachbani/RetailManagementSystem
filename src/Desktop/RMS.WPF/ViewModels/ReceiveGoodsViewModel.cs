using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Application.GetPurchaseOrder;
using RMS.Modules.Purchasing.Application.ReceiveGoods;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class ReceiveGoodsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private Guid _purchaseOrderId;
    private string? _statusMessage;
    private PurchaseOrderItemReadModel? _selectedItem;
    private int _quantityReceived;
    private string? _batchNumber;
    private DateTime? _expiryDate;

    public ReceiveGoodsViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadCommand = new RelayCommand(_ => _ = LoadOrderAsync(_purchaseOrderId));
        ReceiveCommand = new RelayCommand(_ => _ = ReceiveAsync(), _ => SelectedItem is not null && QuantityReceived > 0);
        CancelCommand = new RelayCommand(_ => CloseWithResult(false));
    }

    public ObservableCollection<PurchaseOrderItemReadModel> Items { get; } = new();

    public PurchaseOrderItemReadModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public int QuantityReceived
    {
        get => _quantityReceived;
        set
        {
            _quantityReceived = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? BatchNumber
    {
        get => _batchNumber;
        set { _batchNumber = value; OnPropertyChanged(); }
    }

    public DateTime? ExpiryDate
    {
        get => _expiryDate;
        set { _expiryDate = value; OnPropertyChanged(); }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public ICommand LoadCommand { get; }
    public ICommand ReceiveCommand { get; }
    public ICommand CancelCommand { get; }

    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;

    public async Task LoadOrderAsync(Guid id)
    {
        _purchaseOrderId = id;
        var result = await _mediator.Send(new GetPurchaseOrderQuery(id));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        Items.Clear();
        foreach (var item in result.Value.Items.Where(i => i.ReceivedQuantity < i.Quantity))
            Items.Add(item);
    }

    private async Task ReceiveAsync()
    {
        if (SelectedItem is null) return;
        var command = new ReceiveGoodsCommand(_purchaseOrderId, SelectedItem.ProductId, QuantityReceived, BatchNumber, ExpiryDate);
        var result = await _mediator.Send(command);
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }
        await LoadOrderAsync(_purchaseOrderId);
        StatusMessage = "Goods received successfully.";
        QuantityReceived = 0;
        BatchNumber = null;
        ExpiryDate = null;
        CloseWithResult(true);
    }

    private void CloseWithResult(bool success)
    {
        DialogResult = success;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
