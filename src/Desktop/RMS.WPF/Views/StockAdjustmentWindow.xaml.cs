using System.Windows;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class StockAdjustmentWindow : Window
{
    private readonly StockAdjustmentViewModel _viewModel;

    public StockAdjustmentWindow(StockAdjustmentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public void LoadInventoryItem(InventoryItemReadModel item)
    {
        _viewModel.LoadInventoryItem(item.Id, $"Product: {item.ProductId}", item.CurrentQuantity);
    }
}
