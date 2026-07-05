using System.Windows;

namespace RMS.WPF.Views;

public partial class EditPurchaseOrderWindow : Window
{
    public EditPurchaseOrderWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ViewModels.EditPurchaseOrderViewModel oldVm)
            oldVm.RequestClose -= OnRequestClose;
        if (e.NewValue is ViewModels.EditPurchaseOrderViewModel newVm)
            newVm.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        DialogResult = (DataContext as ViewModels.EditPurchaseOrderViewModel)?.DialogResult;
        Close();
    }
}
