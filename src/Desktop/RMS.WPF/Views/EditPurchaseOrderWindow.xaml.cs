using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class EditPurchaseOrderWindow : Window
{
    public EditPurchaseOrderWindow(EditPurchaseOrderViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        DialogResult = (DataContext as EditPurchaseOrderViewModel)?.DialogResult;
        Close();
    }
}
