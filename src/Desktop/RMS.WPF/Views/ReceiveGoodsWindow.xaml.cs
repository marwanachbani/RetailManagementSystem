using System.Windows;

namespace RMS.WPF.Views;

public partial class ReceiveGoodsWindow : Window
{
    public ReceiveGoodsWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ViewModels.ReceiveGoodsViewModel oldVm)
            oldVm.RequestClose -= OnRequestClose;
        if (e.NewValue is ViewModels.ReceiveGoodsViewModel newVm)
            newVm.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        DialogResult = (DataContext as ViewModels.ReceiveGoodsViewModel)?.DialogResult;
        Close();
    }
}
