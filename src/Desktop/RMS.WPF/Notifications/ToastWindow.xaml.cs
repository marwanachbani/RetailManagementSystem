using System.Windows;
using System.Windows.Input;
using RMS.Modules.Notifications.Domain;

namespace RMS.WPF.Notifications;

public partial class ToastWindow : Window
{
    private readonly Action<Guid> _onClose;
    private readonly System.Windows.Threading.DispatcherTimer _timer;

    public ToastWindow(Notification notification, Action<Guid> onClose)
    {
        InitializeComponent();
        DataContext = notification;
        _onClose = onClose;

        _timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _timer.Tick += (_, _) => Close();
        _timer.Start();

        Loaded += (_, _) => ShowAnimation();
    }

    private void ShowAnimation()
    {
        var animation = new System.Windows.Media.Animation.DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
        };
        BeginAnimation(OpacityProperty, animation);
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _onClose(((Notification)DataContext).Id);
    }

    protected override void OnClosed(EventArgs e)
    {
        _timer.Stop();
        base.OnClosed(e);
    }
}
