using Dapper;
using MediatR;
using RMS.BuildingBlocks.Persistence;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.WPF.Views;
using RMS.BuildingBlocks.Contracts;

namespace RMS.WPF.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IDbConnectionFactory _dbFactory;
    private int _totalProducts;
    private int _activeProducts;
    private int _lowStockCount;
    private int _outOfStockCount;
    private string _statusMessage = "Loading...";

    public DashboardViewModel(IMediator mediator, IDbConnectionFactory dbFactory)
    {
        _mediator = mediator;
        _dbFactory = dbFactory;
        _ = LoadAsync();
    }

    public int TotalProducts
    {
        get => _totalProducts;
        private set { _totalProducts = value; OnPropertyChanged(); }
    }

    public int ActiveProducts
    {
        get => _activeProducts;
        private set { _activeProducts = value; OnPropertyChanged(); }
    }

    public int LowStockCount
    {
        get => _lowStockCount;
        private set { _lowStockCount = value; OnPropertyChanged(); }
    }

    public int OutOfStockCount
    {
        get => _outOfStockCount;
        private set { _outOfStockCount = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public async Task LoadAsync()
    {
        try
        {
            using var connection = _dbFactory.CreateConnection();
            TotalProducts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Products;");
            ActiveProducts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Products WHERE IsActive = 1;");
            LowStockCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM InventoryItems WHERE CurrentQuantity <= LowStockThreshold AND CurrentQuantity > 0;");
            OutOfStockCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM InventoryItems WHERE CurrentQuantity = 0;");
            StatusMessage = $"Database connected. {TotalProducts} products tracked.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading dashboard: {ex.Message}";
        }
    }
}
