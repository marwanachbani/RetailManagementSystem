using System;
using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class EditProductWindow : Window
{
    private readonly EditProductViewModel _viewModel;

    public EditProductWindow(EditProductViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.Saved += OnSaved;
    }

    public async void LoadProduct(Guid productId)
    {
        await _viewModel.LoadProductAsync(productId);
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