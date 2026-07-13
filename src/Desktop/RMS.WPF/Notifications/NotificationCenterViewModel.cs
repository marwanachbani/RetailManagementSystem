using System.Windows;
using System.Windows.Input;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Notifications.Application.Contracts;
using RMS.Modules.Notifications.Domain;
using RMS.WPF.Commands;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Notifications;

public sealed class NotificationCenterViewModel : ViewModelBase
{
    private readonly INotificationRepository _repository;
    private readonly IEventBus _eventBus;
    private string _searchText = string.Empty;
    private NotificationSeverity? _selectedSeverity;
    private string _selectedModule = string.Empty;
    private bool _showRead = true;
    private bool _showUnread = true;
    private Notification? _selectedNotification;

    public NotificationCenterViewModel(
        INotificationRepository repository,
        IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
        LoadNotificationsCommand = new RelayCommand(async _ => await LoadNotificationsAsync());
        MarkAsReadCommand = new RelayCommand(async _ => await MarkAsReadAsync(), _ => SelectedNotification is not null && !SelectedNotification.IsRead);
        MarkAllAsReadCommand = new RelayCommand(async _ => await MarkAllAsReadAsync());
        DeleteNotificationCommand = new RelayCommand(async _ => await DeleteNotificationAsync(), _ => SelectedNotification is not null);
        ClearReadNotificationsCommand = new RelayCommand(async _ => await ClearReadNotificationsAsync());
        CloseCommand = new RelayCommand(_ => Window?.Close());

        LoadNotificationsCommand.Execute(null);
    }

    public ICommand LoadNotificationsCommand { get; }
    public ICommand MarkAsReadCommand { get; }
    public ICommand MarkAllAsReadCommand { get; }
    public ICommand DeleteNotificationCommand { get; }
    public ICommand ClearReadNotificationsCommand { get; }
    public ICommand CloseCommand { get; }

    public List<Notification> Notifications { get; private set; } = new();
    public List<NotificationSeverity> Severities { get; } = Enum.GetValues<NotificationSeverity>().ToList();
    public List<string> Modules { get; } = new()
    {
        "Inventory", "Sales", "Purchasing", "Customers", "Suppliers", "Reports", "Backup", "Audit", "Settings", "System"
    };

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); LoadNotificationsCommand.Execute(null); }
    }

    public NotificationSeverity? SelectedSeverity
    {
        get => _selectedSeverity;
        set { _selectedSeverity = value; OnPropertyChanged(); LoadNotificationsCommand.Execute(null); }
    }

    public string SelectedModule
    {
        get => _selectedModule;
        set { _selectedModule = value; OnPropertyChanged(); LoadNotificationsCommand.Execute(null); }
    }

    public bool ShowRead
    {
        get => _showRead;
        set { _showRead = value; OnPropertyChanged(); LoadNotificationsCommand.Execute(null); }
    }

    public bool ShowUnread
    {
        get => _showUnread;
        set { _showUnread = value; OnPropertyChanged(); LoadNotificationsCommand.Execute(null); }
    }

    public Notification? SelectedNotification
    {
        get => _selectedNotification;
        set { _selectedNotification = value; OnPropertyChanged(); }
    }

    public int UnreadCount { get; private set; }

    public Window? Window { get; set; }

    private async Task LoadNotificationsAsync()
    {
        var all = await _repository.GetAllAsync();
        var filtered = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var lower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(n => n.Title.ToLowerInvariant().Contains(lower) || n.Message.ToLowerInvariant().Contains(lower));
        }

        if (SelectedSeverity.HasValue)
            filtered = filtered.Where(n => n.Severity == SelectedSeverity.Value);

        if (!string.IsNullOrWhiteSpace(SelectedModule))
            filtered = filtered.Where(n => n.RelatedModule.Equals(SelectedModule, StringComparison.OrdinalIgnoreCase));

        filtered = filtered.Where(n => (ShowRead && n.IsRead) || (ShowUnread && !n.IsRead));

        Notifications = filtered.OrderByDescending(n => n.CreatedOn).ToList();
        UnreadCount = all.Count(n => !n.IsRead);
        OnPropertyChanged(nameof(Notifications));
        OnPropertyChanged(nameof(UnreadCount));
    }

    private async Task MarkAsReadAsync()
    {
        if (SelectedNotification is null) return;
        await _repository.MarkAsReadAsync(SelectedNotification.Id);
        await LoadNotificationsAsync();
    }

    private async Task MarkAllAsReadAsync()
    {
        await _repository.MarkAllAsReadAsync();
        await LoadNotificationsAsync();
    }

    private async Task DeleteNotificationAsync()
    {
        if (SelectedNotification is null) return;
        await _repository.DeleteAsync(SelectedNotification.Id);
        await LoadNotificationsAsync();
    }

    private async Task ClearReadNotificationsAsync()
    {
        await _repository.DeleteReadAsync();
        await LoadNotificationsAsync();
    }
}
