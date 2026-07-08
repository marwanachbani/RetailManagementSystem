using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Purchasing.Application.CancelPurchaseOrder;
using RMS.Modules.Purchasing.Application.Contracts;
using RMS.Modules.Purchasing.Application.GetPurchaseOrder;
using RMS.Modules.Purchasing.Application.GetPurchaseOrdersPaged;
using RMS.WPF.Commands;
using RMS.WPF.ReceiptGeneration;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class PurchaseOrdersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _services;
    private readonly PurchaseHistoryViewModel _historyViewModel;
    private readonly IPurchaseOrderDocumentGenerator _documentGenerator;
    private string? _statusMessage;
    private PurchaseOrderReadModel? _selectedOrder;
    private int _pageNumber = 1;
    private int _totalPages = 1;
    private string _searchText = string.Empty;
    private int? _selectedStatusFilter;
    private bool _isHistoryExpanded;

    public PurchaseOrdersViewModel(
        IMediator mediator,
        IServiceProvider services,
        PurchaseHistoryViewModel historyViewModel,
        IPurchaseOrderDocumentGenerator documentGenerator)
    {
        _mediator = mediator;
        _services = services;
        _historyViewModel = historyViewModel;
        _documentGenerator = documentGenerator;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
        NewPurchaseOrderCommand = new RelayCommand(_ => _ = OpenCreateDialog());
        EditPurchaseOrderCommand = new RelayCommand(_ => _ = OpenEditDialog(), _ => SelectedOrder is not null);
        ReceiveGoodsCommand = new RelayCommand(_ => _ = OpenReceiveGoodsDialog(), _ => CanReceiveGoods());
        CancelPurchaseOrderCommand = new RelayCommand(_ => _ = CancelOrderAsync(), _ => CanCancel());
        ViewHistoryCommand = new RelayCommand(_ => _ = ToggleHistoryAsync());
        PrintCommand = new RelayCommand(_ => _ = PrintOrder(), _ => SelectedOrder is not null);
        NextPageCommand = new RelayCommand(_ => _ = NextPageAsync(), _ => PageNumber < TotalPages);
        PreviousPageCommand = new RelayCommand(_ => _ = PreviousPageAsync(), _ => PageNumber > 1);
        SearchCommand = new RelayCommand(_ => _ = LoadAsync());
        _ = LoadAsync();
    }

    public ObservableCollection<PurchaseOrderReadModel> PurchaseOrders { get; } = new();
    public ObservableCollection<string> StatusFilters { get; } = new() { "All", "Draft", "Submitted", "PartiallyReceived", "Completed", "Cancelled" };

    public int PageSize { get; } = 25;
    public PurchaseHistoryViewModel HistoryViewModel => _historyViewModel;

    public PurchaseOrderReadModel? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            _selectedOrder = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
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

    public int PageNumber
    {
        get => _pageNumber;
        private set
        {
            _pageNumber = value;
            OnPropertyChanged();
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            _totalPages = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool IsHistoryExpanded
    {
        get => _isHistoryExpanded;
        set
        {
            _isHistoryExpanded = value;
            OnPropertyChanged();
            if (_isHistoryExpanded)
                _ = HistoryViewModel.LoadAsync();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
        }
    }

    public int? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            _selectedStatusFilter = value;
            OnPropertyChanged();
            _ = LoadAsync();
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand NewPurchaseOrderCommand { get; }
    public ICommand EditPurchaseOrderCommand { get; }
    public ICommand ReceiveGoodsCommand { get; }
    public ICommand CancelPurchaseOrderCommand { get; }
    public ICommand ViewHistoryCommand { get; }
    public ICommand PrintCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand SearchCommand { get; }

    public async Task LoadAsync()
    {
        var searchTerm = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
        var result = await _mediator.Send(new GetPurchaseOrdersPagedQuery(PageNumber, PageSize, searchTerm, SelectedStatusFilter));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        PurchaseOrders.Clear();
        foreach (var order in result.Value.Items)
            PurchaseOrders.Add(order);

        TotalPages = Math.Max(1, result.Value.TotalPages);
        StatusMessage = $"{result.Value.TotalCount} purchase orders";
    }

    private async Task NextPageAsync()
    {
        PageNumber++;
        await LoadAsync();
    }

    private async Task PreviousPageAsync()
    {
        PageNumber--;
        await LoadAsync();
    }

    private async Task OpenCreateDialog()
    {
        var window = (CreatePurchaseOrderWindow)_services.GetService(typeof(CreatePurchaseOrderWindow))!;
        if (window.ShowDialog() == true)
            await LoadAsync();
    }

    private async Task OpenEditDialog()
    {
        if (SelectedOrder is null) return;
        var window = (EditPurchaseOrderWindow)_services.GetService(typeof(EditPurchaseOrderWindow))!;
        var vm = (EditPurchaseOrderViewModel)window.DataContext;
        await vm.LoadOrderAsync(SelectedOrder.Id);
        if (window.ShowDialog() == true)
            await LoadAsync();
    }

    private async Task OpenReceiveGoodsDialog()
    {
        if (SelectedOrder is null) return;
        var window = (ReceiveGoodsWindow)_services.GetService(typeof(ReceiveGoodsWindow))!;
        var vm = (ReceiveGoodsViewModel)window.DataContext;
        await vm.LoadOrderAsync(SelectedOrder.Id);
        if (window.ShowDialog() == true)
            await LoadAsync();
    }

    private Task ToggleHistoryAsync()
    {
        IsHistoryExpanded = !IsHistoryExpanded;
        return Task.CompletedTask;
    }

    private async Task CancelOrderAsync()
    {
        if (SelectedOrder is null) return;
        var result = await _mediator.Send(new CancelPurchaseOrderCommand(SelectedOrder.Id));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }
        await LoadAsync();
    }

    private async Task PrintOrder()
    {
        if (SelectedOrder is null) return;

        // Re-fetch the full order so line items are guaranteed to be populated,
        // even if the paged list query returns a lighter-weight projection.
        var result = await _mediator.Send(new GetPurchaseOrderQuery(SelectedOrder.Id));
        if (result.IsFailure || result.Value is null)
        {
            StatusMessage = result.Error ?? "Could not load purchase order for printing.";
            return;
        }

        try
        {
            var outputDirectory = Path.Combine(App.ProgramDataDirectory, "PurchaseOrders");
            var filePath = await _documentGenerator.GenerateAsync(result.Value, outputDirectory);
            StatusMessage = $"Purchase order saved to {filePath}";

            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to print purchase order: {ex.Message}";
        }
    }

    private bool CanReceiveGoods()
    {
        if (SelectedOrder is null) return false;
        return SelectedOrder.Status is "Submitted" or "PartiallyReceived";
    }

    private bool CanCancel()
    {
        if (SelectedOrder is null) return false;
        return SelectedOrder.Status is "Draft" or "Submitted" or "PartiallyReceived";
    }
}
