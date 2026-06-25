using System;
using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class CreateProductWindow : Window
{
    public CreateProductWindow(CreateProductViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Saved += OnSaved;
    }

    private void OnSaved(object? sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
