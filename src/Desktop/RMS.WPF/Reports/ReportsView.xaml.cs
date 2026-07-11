using System.Windows.Controls;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class ReportsView : UserControl
{
    public ReportsView()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            if (DataContext is ReportsViewModel vm)
                await vm.LoadAllAsync();
        };
    }
}
