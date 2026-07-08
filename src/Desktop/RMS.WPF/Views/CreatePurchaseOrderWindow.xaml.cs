using System;
using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class CreatePurchaseOrderWindow : Window
{
    public CreatePurchaseOrderWindow(CreatePurchaseOrderViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (DataContext is CreatePurchaseOrderViewModel vm)
        {
            _ = vm.LoadProductsAsync();
            _ = vm.LoadSuppliersAsync();
        }
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        DialogResult = (DataContext as CreatePurchaseOrderViewModel)?.DialogResult;
        Close();
    }
}
