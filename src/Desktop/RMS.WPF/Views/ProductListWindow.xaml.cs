using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class ProductListWindow : Window
{
    public ProductListWindow(ProductListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
