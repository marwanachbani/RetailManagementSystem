using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Application.SearchPurchaseOrders;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class PurchaseHistoryViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private string? _statusMessage;
    private string _searchText = string.Empty;
    private int? _selectedStatusFilter;
    private PurchaseOrderReadModel? _selectedOrder;

    public PurchaseHistoryViewModel(IMediator mediator)
    {
        _mediator = mediator;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
        SearchCommand = new RelayCommand(_ => _ = LoadAsync());
        CloseCommand = new RelayCommand(_ => CloseWithResult(false));
        _ = LoadAsync();
    }

    public ObservableCollection<PurchaseOrderReadModel> PurchaseOrders { get; } = new();
    public ObservableCollection<string> StatusFilters { get; } = new() { "All", "Draft", "Submitted", "PartiallyReceived", "Completed", "Cancelled" };

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
    }

    public int? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set { _selectedStatusFilter = value; OnPropertyChanged(); _ = LoadAsync(); }
    }

    public PurchaseOrderReadModel? SelectedOrder
    {
        get => _selectedOrder;
        set { _selectedOrder = value; OnPropertyChanged(); }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand CloseCommand { get; }

    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;

    public async Task LoadAsync()
    {
        var searchTerm = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
        var result = await _mediator.Send(new SearchPurchaseOrdersQuery(searchTerm, SelectedStatusFilter));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        PurchaseOrders.Clear();
        foreach (var order in result.Value)
            PurchaseOrders.Add(order);

        StatusMessage = $"{result.Value.Count} purchase orders";
    }

    private void CloseWithResult(bool success)
    {
        DialogResult = success;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
