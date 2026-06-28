using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class CreateCustomerWindow : Window
{
    public CreateCustomerWindow(CreateCustomerViewModel viewModel)
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
