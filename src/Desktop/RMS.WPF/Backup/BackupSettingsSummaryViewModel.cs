using System.Windows.Input;
using RMS.Modules.Backup.Application.Contracts;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.Backup;

/// <summary>
/// Read-only summary of the active backup configuration sourced from the Settings module.
/// The Backup module never duplicates this configuration.
/// </summary>
public sealed class BackupSettingsSummaryViewModel : BackupViewModelBase
{
    private readonly IBackupSettingsProvider _settings;
    private readonly IDialogService _dialogService;

    private string _backupFolder = string.Empty;
    private bool _automaticBackup;
    private string _frequency = string.Empty;
    private string _time = string.Empty;
    private int _maximumCount;
    private bool _compress;
    private bool _verifyIntegrity;
    private string _statusMessage = "Ready";

    public BackupSettingsSummaryViewModel(IBackupSettingsProvider settings, IDialogService dialogService)
    {
        _settings = settings;
        _dialogService = dialogService;
        RefreshCommand = new RelayCommand(_ => Refresh());
        Refresh();
    }

    public string BackupFolder { get => _backupFolder; private set { _backupFolder = value; OnPropertyChanged(); } }
    public bool AutomaticBackup { get => _automaticBackup; private set { _automaticBackup = value; OnPropertyChanged(); } }
    public string Frequency { get => _frequency; private set { _frequency = value; OnPropertyChanged(); } }
    public string Time { get => _time; private set { _time = value; OnPropertyChanged(); } }
    public int MaximumCount { get => _maximumCount; private set { _maximumCount = value; OnPropertyChanged(); } }
    public bool Compress { get => _compress; private set { _compress = value; OnPropertyChanged(); } }
    public bool VerifyIntegrity { get => _verifyIntegrity; private set { _verifyIntegrity = value; OnPropertyChanged(); } }
    public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; OnPropertyChanged(); } }

    public ICommand RefreshCommand { get; }

    public void Refresh()
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var config = await _settings.GetConfigurationAsync();
            BackupFolder = config.BackupFolder;
            AutomaticBackup = config.AutomaticBackup;
            Frequency = config.Frequency;
            Time = config.Time;
            MaximumCount = config.MaximumCount;
            Compress = config.Compress;
            VerifyIntegrity = config.VerifyIntegrity;
            StatusMessage = "Settings loaded.";
        }
        catch (Exception ex)
        {
            StatusMessage = BackupErrorMapper.ToFriendlyMessage(ex);
        }
    }
}
