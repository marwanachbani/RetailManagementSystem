using System;
using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class CreateSaleWindow : Window
{
    public CreateSaleWindow(CreateSaleViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (DataContext is CreateSaleViewModel vm)
        {
            vm.RequestClose += (_, _) =>
            {
                DialogResult = vm.DialogResult;
                Close();
            };
            _ = vm.InitializeSaleAsync(Guid.NewGuid()); // In real app, use current user
        }
    }
}
