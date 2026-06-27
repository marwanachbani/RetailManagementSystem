using System;
using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class InventoryHistoryWindow : Window
{
    private readonly InventoryHistoryViewModel _viewModel;

    public InventoryHistoryWindow(InventoryHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public void LoadHistory(Guid inventoryItemId)
    {
        _viewModel.LoadHistory(inventoryItemId);
    }
}
