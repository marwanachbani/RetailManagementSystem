using System.Collections.ObjectModel;
using System.Windows.Input;
using Dapper;
using MediatR;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Application.GetInventoryItem;
using RMS.WPF.Commands;
using RMS.WPF.Views;

namespace RMS.WPF.ViewModels;

public sealed class InventoryListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly IServiceProvider _serviceProvider;
    private ObservableCollection<InventoryItemViewModel> _inventoryItems = new();
    private string _searchText = string.Empty;
    private bool _showLowStockOnly;
    private bool _isLoading;
    private bool _hasData;

    public InventoryListViewModel(IMediator mediator, IDbConnectionFactory dbFactory, IServiceProvider serviceProvider)
    {
        _mediator = mediator;
        _dbFactory = dbFactory;
        _serviceProvider = serviceProvider;
        RefreshCommand = new RelayCommand(_ => _ = LoadInventoryAsync());
        StockAdjustmentCommand = new RelayCommand(_ => _ = ShowStockAdjustmentAsync());
        EditStockCommand = new RelayCommand(o => _ = ShowStockAdjustmentAsync((Guid)o!));
        ViewHistoryCommand = new RelayCommand(o => _ = ShowHistoryAsync((Guid)o!));
        _ = LoadInventoryAsync();
    }

    public ObservableCollection<InventoryItemViewModel> InventoryItems
    {
        get => _inventoryItems;
        private set { _inventoryItems = value; OnPropertyChanged(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
    }

    public bool ShowLowStockOnly
    {
        get => _showLowStockOnly;
        set { _showLowStockOnly = value; OnPropertyChanged(); _ = LoadInventoryAsync(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool HasData
    {
        get => _hasData;
        private set { _hasData = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand StockAdjustmentCommand { get; }
    public ICommand EditStockCommand { get; }
    public ICommand ViewHistoryCommand { get; }

    public async Task LoadInventoryAsync()
    {
        IsLoading = true;
        try
        {
            using var connection = _dbFactory.CreateConnection();
            var sql = @"
                SELECT i.Id, i.ProductId, i.CurrentQuantity, i.LowStockThreshold, i.IsActive, i.CreatedAt, i.UpdatedAt,
                       p.Name as ProductName
                FROM InventoryItems i
                JOIN Products p ON i.ProductId = p.Id
                WHERE 1=1
            ";
            if (ShowLowStockOnly)
                sql += " AND i.CurrentQuantity <= i.LowStockThreshold";
            if (!string.IsNullOrWhiteSpace(SearchText))
                sql += " AND p.Name LIKE @search";
            sql += " ORDER BY p.Name";

            var items = await connection.QueryAsync<InventoryItemViewModel>(sql, new { search = "%" + SearchText + "%" });
            InventoryItems.Clear();
            foreach (var item in items)
                InventoryItems.Add(item);
            HasData = InventoryItems.Count > 0;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error loading inventory");
            InventoryItems.Clear();
            HasData = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ShowStockAdjustmentAsync(Guid? id = null)
    {
        var dialog = (StockAdjustmentWindow)_serviceProvider.GetService(typeof(StockAdjustmentWindow))!;
        if (id.HasValue)
        {
            var result = await _mediator.Send(new GetInventoryItemQuery(id.Value));
            if (result.IsSuccess && result.Value is not null)
            {
                dialog.LoadInventoryItem(result.Value);
            }
        }
        if (dialog.ShowDialog() == true)
        {
            await LoadInventoryAsync();
        }
    }

    private async Task ShowHistoryAsync(Guid id)
    {
        var dialog = (InventoryHistoryWindow)_serviceProvider.GetService(typeof(InventoryHistoryWindow))!;
        dialog.LoadHistory(id);
        dialog.ShowDialog();
    }
}

public class InventoryItemViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string StockStatus => CurrentQuantity == 0 ? "OutOfStock" : CurrentQuantity <= LowStockThreshold ? "Low" : "Ok";
    public string StatusText => CurrentQuantity == 0 ? "Out of Stock" : CurrentQuantity <= LowStockThreshold ? "Low" : "OK";
}
