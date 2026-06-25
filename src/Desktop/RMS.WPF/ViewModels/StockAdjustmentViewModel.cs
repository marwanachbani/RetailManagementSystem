using System.Windows.Input;
using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.AdjustStock;
using RMS.Modules.Inventory.Application.DecreaseStock;
using RMS.Modules.Inventory.Application.IncreaseStock;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class StockAdjustmentViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private Guid _inventoryItemId;
    private string _productInfo = "";
    private int _currentQuantity;
    private int _adjustmentAmount;
    private int _newQuantity;
    private string _reason = "";
    private string? _statusMessage;
    private string _selectedOperation = "Increase";

    public StockAdjustmentViewModel(IMediator mediator)
    {
        _mediator = mediator;
        SaveCommand = new RelayCommand(_ => _ = SaveAsync(), _ => CanSave);
        CancelCommand = new RelayCommand(_ => CloseWithResult(false));
    }

    public void LoadInventoryItem(Guid inventoryItemId, string productInfo, int currentQuantity)
    {
        _inventoryItemId = inventoryItemId;
        ProductInfo = productInfo;
        CurrentQuantity = currentQuantity;
        AdjustmentAmount = 0;
        NewQuantity = currentQuantity;
        Reason = "";
        StatusMessage = null;
    }

    public string ProductInfo
    {
        get => _productInfo;
        private set
        {
            _productInfo = value;
            OnPropertyChanged();
        }
    }

    public int CurrentQuantity
    {
        get => _currentQuantity;
        private set
        {
            _currentQuantity = value;
            OnPropertyChanged();
        }
    }

    public int AdjustmentAmount
    {
        get => _adjustmentAmount;
        set
        {
            _adjustmentAmount = value;
            OnPropertyChanged();
            if (SelectedOperation == "Increase")
                NewQuantity = CurrentQuantity + value;
            else if (SelectedOperation == "Decrease")
                NewQuantity = CurrentQuantity - value;
        }
    }

    public int NewQuantity
    {
        get => _newQuantity;
        set
        {
            _newQuantity = value;
            OnPropertyChanged();
        }
    }

    public string Reason
    {
        get => _reason;
        set
        {
            _reason = value;
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

    public string SelectedOperation
    {
        get => _selectedOperation;
        set
        {
            _selectedOperation = value;
            OnPropertyChanged();
            AdjustmentAmount = 0;
            if (value == "Adjust")
                NewQuantity = CurrentQuantity;
        }
    }

    public List<string> Operations { get; } = new() { "Increase", "Decrease", "Adjust" };

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private bool CanSave => !string.IsNullOrWhiteSpace(Reason) &&
        (SelectedOperation != "Adjust" || NewQuantity >= 0) &&
        (SelectedOperation != "Decrease" || AdjustmentAmount <= CurrentQuantity);

    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;

    private async Task SaveAsync()
    {
        Result result;
        switch (SelectedOperation)
        {
            case "Increase":
                result = await _mediator.Send(new IncreaseStockCommand(_inventoryItemId, AdjustmentAmount, Reason));
                break;
            case "Decrease":
                result = await _mediator.Send(new DecreaseStockCommand(_inventoryItemId, AdjustmentAmount, Reason));
                break;
            case "Adjust":
                result = await _mediator.Send(new AdjustStockCommand(_inventoryItemId, NewQuantity, Reason));
                break;
            default:
                StatusMessage = "Invalid operation.";
                return;
        }

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
