using System.Windows;

namespace RMS.WPF.Notifications;

public partial class NotificationDetailsWindow : Window
{
    public NotificationDetailsWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
