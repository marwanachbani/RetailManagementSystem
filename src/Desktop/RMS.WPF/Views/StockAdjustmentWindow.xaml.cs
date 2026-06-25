using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class StockAdjustmentWindow : Window
{
    public StockAdjustmentWindow()
    {
        InitializeComponent();
    }

    public void LoadInventoryItem(RMS.Modules.Inventory.Application.Contracts.InventoryItemReadModel item)
    {
        if (DataContext is StockAdjustmentViewModel vm)
        {
            vm.LoadInventoryItem(item.Id, $"Product: {item.ProductId}", item.CurrentQuantity);
        }
    }
}
