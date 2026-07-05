using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class EditSupplierWindow : Window
{
    public EditSupplierWindow(EditSupplierViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += (_, success) =>
        {
            DialogResult = success;
            Close();
        };
    }
}
