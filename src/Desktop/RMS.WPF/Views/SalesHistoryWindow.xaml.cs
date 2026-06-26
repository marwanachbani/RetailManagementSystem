using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class SalesHistoryWindow : Window
{
    public SalesHistoryWindow()
    {
        InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (DataContext is SalesHistoryViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
