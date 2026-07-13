using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;

namespace RMS.WPF.Notifications;

public sealed class ToastNotificationManager
{
    private readonly INotificationRepository _repository;
    private readonly Window _mainWindow;
    private readonly Dictionary<Guid, ToastWindow> _activeToasts = new();
    private readonly object _lock = new();
    private int _toastCount;

    public ToastNotificationManager(INotificationRepository repository, Window mainWindow)
    {
        _repository = repository;
        _mainWindow = mainWindow;
    }

    public void ShowToast(Notification notification)
    {
        lock (_lock)
        {
            var toast = new ToastWindow(notification, CloseToast);
            _activeToasts[notification.Id] = toast;
            _toastCount++;
            var offset = _toastCount * 10;
            toast.Left = _mainWindow.Left + _mainWindow.Width - toast.Width - 20;
            toast.Top = _mainWindow.Top + _mainWindow.Height - toast.Height - 60 - offset;
            toast.Show();
        }
    }

    private void CloseToast(Guid notificationId)
    {
        lock (_lock)
        {
            if (_activeToasts.TryGetValue(notificationId, out var toast))
            {
                toast.Close();
                _activeToasts.Remove(notificationId);
            }
        }
    }

    public void CloseAll()
    {
        lock (_lock)
        {
            foreach (var toast in _activeToasts.Values)
                toast.Close();
            _activeToasts.Clear();
        }
    }
}
