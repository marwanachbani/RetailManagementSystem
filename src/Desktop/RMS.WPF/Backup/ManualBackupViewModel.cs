using System.Windows.Input;
using RMS.Modules.Backup.Application.Contracts;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.Backup;

public sealed class ManualBackupViewModel : BackupViewModelBase
{
    private readonly IBackupService _backupService;
    private readonly IBackupSettingsProvider _settings;
    private readonly IDialogService _dialogService;

    private string? _notes;
    private bool _compress = true;
    private bool _isBusy;
    private int _progressPercent;
    private string _progressStage = string.Empty;
    private string _statusMessage = "Ready";
    private CancellationTokenSource? _cts;

    public ManualBackupViewModel(IBackupService backupService, IBackupSettingsProvider settings, IDialogService dialogService)
    {
        _backupService = backupService;
        _settings = settings;
        _dialogService = dialogService;

        CreateBackupCommand = new RelayCommand(_ => _ = CreateAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => Cancel(), _ => IsBusy);
        RefreshCommand = new RelayCommand(_ => _ = LoadDefaultsAsync());
        _ = LoadDefaultsAsync();
    }

    public string? Notes { get => _notes; set { _notes = value; OnPropertyChanged(); } }
    public bool Compress { get => _compress; set { _compress = value; OnPropertyChanged(); } }
    public bool IsBusy { get => _isBusy; private set { _isBusy = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } }
    public int ProgressPercent { get => _progressPercent; private set { _progressPercent = value; OnPropertyChanged(); } }
    public string ProgressStage { get => _progressStage; private set { _progressStage = value; OnPropertyChanged(); } }
    public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; OnPropertyChanged(); } }

    public ICommand CreateBackupCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task LoadDefaultsAsync()
    {
        try
        {
            var config = await _settings.GetConfigurationAsync();
            Compress = config.Compress;
        }
        catch
        {
            // Defaults remain; the user can still create a backup.
        }
    }

    private async Task CreateAsync()
    {
        _cts = new CancellationTokenSource();
        IsBusy = true;
        ProgressPercent = 0;
        StatusMessage = "Starting backup…";
        try
        {
            var progress = new Progress<RMS.Modules.Backup.Application.Models.BackupProgress>(p =>
            {
                ProgressPercent = p.Percent;
                ProgressStage = p.Stage;
                StatusMessage = p.Message;
            });

            var result = await _backupService.CreateBackupAsync(
                Notes, progress, _cts.Token);

            _dialogService.ShowInfo($"Backup '{result.FileName}' created ({(result.Compressed ? "ZIP" : "Folder")}, {BackupDashboardViewModel.FormatSize(result.Size)}).", "Backup Created");
            StatusMessage = "Backup completed successfully.";
            Notes = string.Empty;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Backup cancelled.";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(BackupErrorMapper.ToFriendlyMessage(ex), "Backup Failed");
            StatusMessage = "Backup failed.";
        }
        finally
        {
            IsBusy = false;
            ProgressPercent = 0;
            ProgressStage = string.Empty;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void Cancel()
    {
        try { _cts?.Cancel(); } catch { /* best effort */ }
    }
}
