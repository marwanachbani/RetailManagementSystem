using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Application.GetInventoryHistory;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class InventoryHistoryViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private string? _statusMessage;

    public InventoryHistoryViewModel(IMediator mediator)
    {
        _mediator = mediator;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
    }

    public ObservableCollection<InventoryTransactionReadModel> Transactions { get; } = new();

    public Guid InventoryItemId { get; private set; }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand { get; }

    public void LoadHistory(Guid inventoryItemId)
    {
        InventoryItemId = inventoryItemId;
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        if (InventoryItemId == Guid.Empty)
            return;

        var result = await _mediator.Send(new GetInventoryHistoryQuery(InventoryItemId));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        Transactions.Clear();
        foreach (var transaction in result.Value)
            Transactions.Add(transaction);

        StatusMessage = $"{Transactions.Count} transactions";
    }
}
