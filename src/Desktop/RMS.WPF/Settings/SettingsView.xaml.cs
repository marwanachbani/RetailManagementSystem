using System.Windows.Controls;

namespace RMS.WPF.Settings;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is SettingsViewModel vm)
                _ = vm.LoadAsync();
        };
    }
}
