using System.Windows.Input;
using RMS.WPF.Commands;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Backup;

/// <summary>
/// Host view model for the Backup &amp; Restore module. Owns the five sub-screens
/// (Dashboard, Manual Backup, Restore, History, Settings Summary) and provides the
/// in-module navigation used by the sub view models.
/// </summary>
public sealed class BackupAndRestoreViewModel : ViewModelBase
{
    private readonly BackupDashboardViewModel _dashboard;
    private readonly ManualBackupViewModel _manual;
    private readonly RestoreBackupViewModel _restore;
    private readonly BackupHistoryViewModel _history;
    private readonly BackupSettingsSummaryViewModel _settings;
    private int _selectedTabIndex;

    public BackupAndRestoreViewModel(
        BackupDashboardViewModel dashboard,
        ManualBackupViewModel manual,
        RestoreBackupViewModel restore,
        BackupHistoryViewModel history,
        BackupSettingsSummaryViewModel settings)
    {
        _dashboard = dashboard;
        _manual = manual;
        _restore = restore;
        _history = history;
        _settings = settings;

        foreach (var vm in new BackupViewModelBase[] { dashboard, manual, restore, history, settings })
            vm.Navigate = NavigateTo;

        RefreshAllCommand = new RelayCommand(_ => RefreshAll());
        SelectedTabIndex = 0;
    }

    public BackupDashboardViewModel Dashboard => _dashboard;
    public ManualBackupViewModel Manual => _manual;
    public RestoreBackupViewModel Restore => _restore;
    public BackupHistoryViewModel History => _history;
    public BackupSettingsSummaryViewModel Settings => _settings;

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set { _selectedTabIndex = value; OnPropertyChanged(); }
    }

    public ICommand RefreshAllCommand { get; }

    public void NavigateTo(BackupScreen screen) => SelectedTabIndex = (int)screen;

    private void RefreshAll()
    {
        _dashboard.Refresh();
        _manual.RefreshCommand.Execute(null);
        _restore.Refresh();
        _history.Refresh();
        _settings.Refresh();
    }
}
