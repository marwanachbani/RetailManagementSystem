using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Application.Models;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.Backup;

public sealed class BackupDashboardViewModel : BackupViewModelBase
{
    private readonly IBackupService _backupService;
    private readonly IDialogService _dialogService;

    private string _lastBackup = "Never";
    private string _nextScheduled = "Not scheduled";
    private string _backupFolder = string.Empty;
    private int _totalBackups;
    private string _totalBackupSize = "0 KB";
    private string _automaticBackupStatus = "Disabled";
    private string _statusMessage = "Ready";
    private bool _isBusy;

    public BackupDashboardViewModel(IBackupService backupService, IDialogService dialogService)
    {
        _backupService = backupService;
        _dialogService = dialogService;

        RefreshCommand = new RelayCommand(_ => Refresh());
        CreateBackupCommand = new RelayCommand(_ => _ = QuickCreateAsync());
        RestoreCommand = new RelayCommand(_ => Navigate?.Invoke(BackupScreen.Restore));
        VerifyCommand = new RelayCommand(_ => _ = VerifyLastAsync());
        OpenFolderCommand = new RelayCommand(_ => OpenFolder());
    }

    public string LastBackup { get => _lastBackup; private set { _lastBackup = value; OnPropertyChanged(); } }
    public string NextScheduled { get => _nextScheduled; private set { _nextScheduled = value; OnPropertyChanged(); } }
    public string BackupFolder { get => _backupFolder; private set { _backupFolder = value; OnPropertyChanged(); } }
    public int TotalBackups { get => _totalBackups; private set { _totalBackups = value; OnPropertyChanged(); } }
    public string TotalBackupSize { get => _totalBackupSize; private set { _totalBackupSize = value; OnPropertyChanged(); } }
    public string AutomaticBackupStatus { get => _automaticBackupStatus; private set { _automaticBackupStatus = value; OnPropertyChanged(); } }
    public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; OnPropertyChanged(); } }
    public bool IsBusy { get => _isBusy; private set { _isBusy = value; OnPropertyChanged(); } }

    public ICommand RefreshCommand { get; }
    public ICommand CreateBackupCommand { get; }
    public ICommand RestoreCommand { get; }
    public ICommand VerifyCommand { get; }
    public ICommand OpenFolderCommand { get; }

    public void Refresh()
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var dashboard = await _backupService.GetDashboardAsync();
            LastBackup = dashboard.LastBackupDate?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
            NextScheduled = dashboard.NextScheduledBackup?.ToString("yyyy-MM-dd HH:mm") ?? "Not scheduled";
            BackupFolder = dashboard.BackupFolder;
            TotalBackups = dashboard.TotalBackups;
            TotalBackupSize = FormatSize(dashboard.TotalSize);
            AutomaticBackupStatus = dashboard.AutomaticBackupEnabled
                ? $"Enabled ({dashboard.Frequency})"
                : "Disabled";
            StatusMessage = "Dashboard loaded.";
        }
        catch (Exception ex)
        {
            StatusMessage = BackupErrorMapper.ToFriendlyMessage(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task QuickCreateAsync()
    {
        IsBusy = true;
        StatusMessage = "Creating backup…";
        try
        {
            var result = await _backupService.CreateBackupAsync(null);
            _dialogService.ShowInfo($"Backup '{result.FileName}' created successfully.", "Backup Created");
            StatusMessage = "Backup created successfully.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(BackupErrorMapper.ToFriendlyMessage(ex), "Backup Failed");
            StatusMessage = "Backup failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task VerifyLastAsync()
    {
        IsBusy = true;
        StatusMessage = "Verifying last backup…";
        try
        {
            var history = await _backupService.GetHistoryAsync();
            var last = history.FirstOrDefault();
            if (last is null)
            {
                _dialogService.ShowWarning("There are no backups to verify.", "Verify");
                return;
            }

            var verification = await _backupService.VerifyBackupAsync(last.FilePath);
            if (verification.IsValid)
                _dialogService.ShowInfo($"Backup '{last.FileName}' is valid.", "Verification Passed");
            else
                _dialogService.ShowError(verification.Error ?? "Backup is invalid.", "Verification Failed");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(BackupErrorMapper.ToFriendlyMessage(ex), "Verification Failed");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenFolder()
    {
        if (string.IsNullOrWhiteSpace(BackupFolder) || !Directory.Exists(BackupFolder))
        {
            _dialogService.ShowWarning("The backup folder does not exist yet.", "Open Folder");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(BackupFolder) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(BackupErrorMapper.ToFriendlyMessage(ex), "Open Folder");
        }
    }

    public static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        int i = 0;
        while (value >= 1024 && i < suffixes.Length - 1)
        {
            value /= 1024;
            i++;
        }

        return $"{value:0.##} {suffixes[i]}";
    }
}
