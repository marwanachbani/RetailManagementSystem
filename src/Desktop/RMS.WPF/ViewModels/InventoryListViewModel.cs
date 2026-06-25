using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Application.GetInventoryPaged;
using RMS.WPF.Commands;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class InventoryListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _services;
    private string? _searchTerm;
    private InventoryItemReadModel? _selectedItem;
    private string? _statusMessage;
    private int _pageNumber = 1;
    private int _totalPages = 1;

    public InventoryListViewModel(IMediator mediator, IServiceProvider services)
    {
        _mediator = mediator;
        _services = services;
        RefreshCommand = new RelayCommand(_ => _ = LoadAsync());
        SearchCommand = new RelayCommand(_ => _ = SearchAsync());
        AdjustStockCommand = new RelayCommand(_ => _ = OpenAdjustmentDialog(), _ => SelectedItem is not null);
        ViewHistoryCommand = new RelayCommand(_ => _ = OpenHistoryDialog(), _ => SelectedItem is not null);
        NextPageCommand = new RelayCommand(_ => _ = NextPageAsync(), _ => PageNumber < TotalPages);
        PreviousPageCommand = new RelayCommand(_ => _ = PreviousPageAsync(), _ => PageNumber > 1);
        _ = LoadAsync();
    }

    public ObservableCollection<InventoryItemReadModel> Items { get; } = new();
    public int PageSize { get; } = 25;

    public string? SearchTerm
    {
        get => _searchTerm;
        set
        {
            _searchTerm = value;
            OnPropertyChanged();
        }
    }

    public InventoryItemReadModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
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

    public ICommand RefreshCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AdjustStockCommand { get; }
    public ICommand ViewHistoryCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    public async Task LoadAsync()
    {
        var result = await _mediator.Send(new GetInventoryPagedQuery(PageNumber, PageSize, SearchTerm, false));
        if (result.IsFailure)
        {
            StatusMessage = result.Error;
            return;
        }

        Items.Clear();
        foreach (var item in result.Value.Items)
            Items.Add(item);

        TotalPages = Math.Max(1, result.Value.TotalPages);
        StatusMessage = $"{result.Value.TotalCount} inventory items";
    }

    private async Task SearchAsync()
    {
        PageNumber = 1;
        await LoadAsync();
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

    private async Task OpenAdjustmentDialog()
    {
        if (SelectedItem is null) return;

        var window = (StockAdjustmentWindow)_services.GetService(typeof(StockAdjustmentWindow))!;
        window.LoadInventoryItem(SelectedItem);
        if (window.ShowDialog() == true)
            await LoadAsync();
    }

    private async Task OpenHistoryDialog()
    {
        if (SelectedItem is null) return;

        var window = (InventoryHistoryWindow)_services.GetService(typeof(InventoryHistoryWindow))!;
        window.LoadHistory(SelectedItem.Id);
        window.ShowDialog();
    }
}
