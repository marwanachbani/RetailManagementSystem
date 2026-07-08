using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class SaleDetailsWindow : Window
{
    public SaleDetailsWindow(SaleDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
