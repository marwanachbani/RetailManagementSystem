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

public sealed class BackupHistoryViewModel : BackupViewModelBase
{
    private readonly IBackupService _backupService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<BackupHistoryEntry> _backups = new();
    private BackupHistoryEntry? _selectedBackup;
    private bool _isBusy;
    private string _statusMessage = "Ready";

    public BackupHistoryViewModel(IBackupService backupService, IDialogService dialogService)
    {
        _backupService = backupService;
        _dialogService = dialogService;

        RefreshCommand = new RelayCommand(_ => Refresh());
        RestoreCommand = new RelayCommand(_ => _ = RestoreAsync(), _ => SelectedBackup is not null && !IsBusy);
        VerifyCommand = new RelayCommand(_ => _ = VerifyAsync(), _ => SelectedBackup is not null && !IsBusy);
        DeleteCommand = new RelayCommand(_ => _ = DeleteAsync(), _ => SelectedBackup is not null && !IsBusy);
        OpenFolderCommand = new RelayCommand(_ => OpenFolder());
        Refresh();
    }

    public ObservableCollection<BackupHistoryEntry> Backups { get => _backups; private set { _backups = value; OnPropertyChanged(); } }
    public BackupHistoryEntry? SelectedBackup { get => _selectedBackup; set { _selectedBackup = value; OnPropertyChanged(); } }
    public bool IsBusy { get => _isBusy; private set { _isBusy = value; OnPropertyChanged(); } }
    public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; OnPropertyChanged(); } }

    public ICommand RefreshCommand { get; }
    public ICommand RestoreCommand { get; }
    public ICommand VerifyCommand { get; }
    public ICommand DeleteCommand { get; }
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
            var entries = await _backupService.GetHistoryAsync();
            Backups = new ObservableCollection<BackupHistoryEntry>(entries);
            StatusMessage = $"{entries.Count} backup(s) found.";
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

    private async Task RestoreAsync()
    {
        if (SelectedBackup is null) return;

        var confirm = _dialogService.Confirm(
            $"Restoring '{SelectedBackup.FileName}' will replace the current database and files.\n\n" +
            "A safety backup of the current data will be created automatically before the restore.\n\n" +
            "The application will restart when the restore completes. Do you want to continue?",
            "Confirm Restore");
        if (!confirm) return;

        IsBusy = true;
        try
        {
            var result = await _backupService.RestoreAsync(
                new RestoreRequest { BackupPath = SelectedBackup.FilePath, Notes = "Restore via History", VerifyBeforeRestore = true });

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

    private async Task VerifyAsync()
    {
        if (SelectedBackup is null) return;
        IsBusy = true;
        try
        {
            var result = await _backupService.VerifyBackupAsync(SelectedBackup.FilePath);
            if (result.IsValid)
                _dialogService.ShowInfo($"Backup '{SelectedBackup.FileName}' is valid.", "Verification Passed");
            else
                _dialogService.ShowError(result.Error ?? "Backup is invalid.", "Verification Failed");
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

    private async Task DeleteAsync()
    {
        if (SelectedBackup is null) return;
        var confirm = _dialogService.Confirm(
            $"Delete backup '{SelectedBackup.FileName}'? This cannot be undone.",
            "Delete Backup");
        if (!confirm) return;

        IsBusy = true;
        try
        {
            var result = await _backupService.DeleteBackupAsync(SelectedBackup.Id);
            if (result.IsFailure)
            {
                _dialogService.ShowError(result.Error ?? "Could not delete the backup.", "Delete Failed");
                return;
            }

            _dialogService.ShowInfo("The backup was deleted.", "Deleted");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(BackupErrorMapper.ToFriendlyMessage(ex), "Delete Failed");
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
        }

        Application.Current.Shutdown();
    }
}
