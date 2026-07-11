using System.ComponentModel;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.Commands;
using RMS.WPF.Services;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Settings;

/// <summary>
/// Base for every Settings section. Provides dirty tracking (via the view model's
/// own PropertyChanged events), a notification banner, and the Save / Cancel /
/// Restore Defaults commands that the master Settings toolbar binds to.
/// </summary>
public abstract class SettingsSectionViewModelBase : ViewModelBase
{
    private bool _isDirty;
    private bool _suppressDirty;
    private string _notification = string.Empty;
    private bool _hasError;
    private readonly System.Timers.Timer _notificationTimer = new(3500) { AutoReset = false };

    protected SettingsSectionViewModelBase(IMediator mediator, IDialogService dialogService)
    {
        Mediator = mediator;
        DialogService = dialogService;

        SaveCommand = new RelayCommand(_ => _ = SaveAsync());
        CancelCommand = new RelayCommand(_ => Cancel());
        RestoreDefaultsCommand = new RelayCommand(_ => _ = RestoreDefaultsAsync());

        PropertyChanged += OnSelfPropertyChanged;
        _notificationTimer.Elapsed += (_, _) =>
        {
            var app = System.Windows.Application.Current;
            if (app?.Dispatcher is { } dispatcher)
                dispatcher.Invoke(ClearNotification);
            else
                ClearNotification();
        };
    }

    protected IMediator Mediator { get; }
    protected IDialogService DialogService { get; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RestoreDefaultsCommand { get; }

    public abstract string Title { get; }
    public abstract string Description { get; }

    /// <summary>
    /// Invoked after a successful reset so the host can reload every section
    /// (ResetSettings resets the entire catalog, not just this section).
    /// </summary>
    public Action? RequestGlobalReload { get; set; }

    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (_isDirty == value) return;
            _isDirty = value;
            OnPropertyChanged();
        }
    }

    public string Notification
    {
        get => _notification;
        private set { _notification = value; OnPropertyChanged(); }
    }

    public bool HasError
    {
        get => _hasError;
        private set { _hasError = value; OnPropertyChanged(); }
    }

    protected void BeginLoad() => _suppressDirty = true;
    protected void EndLoad() { _suppressDirty = false; IsDirty = false; }

    protected void ShowSuccess(string message)
    {
        HasError = false;
        Notification = message;
        _notificationTimer.Stop();
        _notificationTimer.Start();
    }

    protected void ShowError(string message)
    {
        HasError = true;
        Notification = message;
        _notificationTimer.Stop();
        _notificationTimer.Start();
    }

    private void ClearNotification()
    {
        HasError = false;
        Notification = string.Empty;
    }

    private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppressDirty) return;
        if (e.PropertyName == nameof(IsDirty) ||
            e.PropertyName == nameof(Notification) ||
            e.PropertyName == nameof(HasError)) return;
        IsDirty = true;
    }

    public abstract void Load(SettingsModel model);
    protected abstract SettingsModel Snapshot { get; set; }
    public abstract Task SaveAsync();
    public abstract void Cancel();
    public abstract Task RestoreDefaultsAsync();
}
