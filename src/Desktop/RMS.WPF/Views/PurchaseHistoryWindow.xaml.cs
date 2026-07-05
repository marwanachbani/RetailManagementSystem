using System.Windows;

namespace RMS.WPF.Views;

public partial class PurchaseHistoryWindow : Window
{
    public PurchaseHistoryWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ViewModels.PurchaseHistoryViewModel oldVm)
            oldVm.RequestClose -= OnRequestClose;
        if (e.NewValue is ViewModels.PurchaseHistoryViewModel newVm)
            newVm.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        DialogResult = (DataContext as ViewModels.PurchaseHistoryViewModel)?.DialogResult;
        Close();
    }
}
