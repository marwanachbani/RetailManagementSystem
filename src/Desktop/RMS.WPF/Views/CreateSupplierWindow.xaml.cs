using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class CreateSupplierWindow : Window
{
    public CreateSupplierWindow(CreateSupplierViewModel viewModel)
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
