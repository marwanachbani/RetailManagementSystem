using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class InventoryHistoryWindow : Window
{
    public InventoryHistoryWindow()
    {
        InitializeComponent();
    }

    public void LoadHistory(Guid inventoryItemId)
    {
        if (DataContext is InventoryHistoryViewModel vm)
        {
            vm.LoadHistory(inventoryItemId);
        }
    }
}
