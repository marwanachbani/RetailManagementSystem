using System.Windows;

namespace RMS.WPF.Views;

public partial class CreatePurchaseOrderWindow : Window
{
    public CreatePurchaseOrderWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ViewModels.CreatePurchaseOrderViewModel oldVm)
            oldVm.RequestClose -= OnRequestClose;
        if (e.NewValue is ViewModels.CreatePurchaseOrderViewModel newVm)
            newVm.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        DialogResult = (DataContext as ViewModels.CreatePurchaseOrderViewModel)?.DialogResult;
        Close();
    }
}
