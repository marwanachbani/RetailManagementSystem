using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class EditCustomerWindow : Window
{
    public EditCustomerWindow(EditCustomerViewModel viewModel)
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
