using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using RMS.Modules.Backup.Application.Contracts;
using RMS.Modules.Backup.Application.Models;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.Backup;

public sealed class RestoreBackupViewModel : BackupViewModelBase
{
    private readonly IBackupService _backupService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<BackupHistoryEntry> _backups = new();
    private BackupHistoryEntry? _selectedBackup;
    private BackupMetadata? _details;
    private string _verificationStatus = "Select a backup and verify its integrity.";
    private bool _isVerified;
    private bool _isBusy;
    private int _progressPercent;
    private string _progressStage = string.Empty;
    private string _statusMessage = "Ready";

    public RestoreBackupViewModel(IBackupService backupService, IDialogService dialogService)
    {
        _backupService = backupService;
        _dialogService = dialogService;

        RefreshCommand = new RelayCommand(_ => Refresh());
        VerifyCommand = new RelayCommand(_ => _ = VerifyAsync(), _ => SelectedBackup is not null && !IsBusy);
        RestoreCommand = new RelayCommand(_ => _ = RestoreAsync(), _ => SelectedBackup is not null && !IsBusy);
        OpenFolderCommand = new RelayCommand(_ => OpenFolder());
        Refresh();
    }

    public ObservableCollection<BackupHistoryEntry> Backups { get => _backups; private set { _backups = value; OnPropertyChanged(); } }
    public BackupHistoryEntry? SelectedBackup { get => _selectedBackup; set { _selectedBackup = value; OnPropertyChanged(); _ = LoadDetailsAsync(); CommandManager.InvalidateRequerySuggested(); } }
    public BackupMetadata? Details { get => _details; private set { _details = value; OnPropertyChanged(); } }
    public string VerificationStatus { get => _verificationStatus; private set { _verificationStatus = value; OnPropertyChanged(); } }
    public bool IsVerified { get => _isVerified; private set { _isVerified = value; OnPropertyChanged(); } }
    public bool IsBusy { get => _isBusy; private set { _isBusy = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } }
    public int ProgressPercent { get => _progressPercent; private set { _progressPercent = value; OnPropertyChanged(); } }
    public string ProgressStage { get => _progressStage; private set { _progressStage = value; OnPropertyChanged(); } }
    public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; OnPropertyChanged(); } }

    public ICommand RefreshCommand { get; }
    public ICommand VerifyCommand { get; }
    public ICommand RestoreCommand { get; }
    public ICommand OpenFolderCommand { get; }

    public void Refresh()
    {
        _ = LoadBackupsAsync();
    }

    private async Task LoadBackupsAsync()
    {
        IsBusy = true;
        try
        {
            var entries = await _backupService.GetHistoryAsync();
            Backups = new ObservableCollection<BackupHistoryEntry>(entries);
            StatusMessage = $"{entries.Count} backup(s) available.";
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

    private async Task LoadDetailsAsync()
    {
        Details = null;
        IsVerified = false;
        VerificationStatus = "Select a backup and verify its integrity.";
        if (SelectedBackup is null) return;

        try
        {
            Details = await _backupService.GetBackupDetailsAsync(SelectedBackup.FilePath);
        }
        catch (Exception ex)
        {
            VerificationStatus = BackupErrorMapper.ToFriendlyMessage(ex);
        }
    }

    private async Task VerifyAsync()
    {
        if (SelectedBackup is null) return;
        IsBusy = true;
        VerificationStatus = "Verifying…";
        try
        {
            var result = await _backupService.VerifyBackupAsync(SelectedBackup.FilePath);
            IsVerified = result.IsValid;
            VerificationStatus = result.IsValid
                ? "Backup integrity verified — ready to restore."
                : $"Verification failed: {result.Error}";
        }
        catch (Exception ex)
        {
            IsVerified = false;
            VerificationStatus = BackupErrorMapper.ToFriendlyMessage(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RestoreAsync()
    {
        if (SelectedBackup is null) return;

        if (Details is null)
            Details = await _backupService.GetBackupDetailsAsync(SelectedBackup.FilePath);

        var confirm = _dialogService.Confirm(
            $"Restoring '{SelectedBackup.FileName}' will replace the current database and files.\n\n" +
            "A safety backup of the current data will be created automatically before the restore.\n\n" +
            "The application will restart when the restore completes. Do you want to continue?",
            "Confirm Restore");
        if (!confirm) return;

        IsBusy = true;
        ProgressPercent = 0;
        StatusMessage = "Restoring…";
        try
        {
            var progress = new Progress<RestoreProgress>(p =>
            {
                ProgressPercent = p.Percent;
                ProgressStage = p.Stage;
                StatusMessage = p.Message;
            });

            var result = await _backupService.RestoreAsync(
                new RestoreRequest { BackupPath = SelectedBackup.FilePath, Notes = "Restore via Backup & Restore", VerifyBeforeRestore = true },
                progress);

            if (!result.Succeeded)
            {
                _dialogService.ShowError(result.Error ?? "Restore failed.", "Restore Failed");
                StatusMessage = "Restore failed.";
                return;
            }

            _dialogService.ShowInfo(
                "The backup has been restored successfully. The application will now restart to apply the changes.",
                "Restore Complete");
            RestartApplication();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(BackupErrorMapper.ToFriendlyMessage(ex), "Restore Failed");
            StatusMessage = "Restore failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenFolder()
    {
        if (SelectedBackup is null) return;
        var dir = Path.GetDirectoryName(SelectedBackup.FilePath);
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) return;
        try { Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true }); }
        catch (Exception ex) { _dialogService.ShowError(BackupErrorMapper.ToFriendlyMessage(ex), "Open Folder"); }
    }

    private static void RestartApplication()
    {
        try
        {
            var exe = Process.GetCurrentProcess().MainModule?.FileName;
            if (exe is not null) Process.Start(exe);
        }
        catch
        {
            // If we cannot launch a new instance, still shut down cleanly.
        }

        Application.Current.Shutdown();
    }
}
