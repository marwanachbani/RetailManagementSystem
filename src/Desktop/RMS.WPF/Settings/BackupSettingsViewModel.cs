using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateBackupSettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class BackupSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public BackupSettingsViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService) { }

    public override string Title => "Backups";
    public override string Description => "Automated database backup policy.";

    private bool _automaticBackup;
    public bool AutomaticBackup { get => _automaticBackup; set { _automaticBackup = value; OnPropertyChanged(); } }
    private string _frequency = string.Empty;
    public string Frequency { get => _frequency; set { _frequency = value; OnPropertyChanged(); } }
    private string _time = string.Empty;
    public string Time { get => _time; set { _time = value; OnPropertyChanged(); } }
    private int _maximumCount;
    public int MaximumCount { get => _maximumCount; set { _maximumCount = value; OnPropertyChanged(); } }
    private bool _compress;
    public bool Compress { get => _compress; set { _compress = value; OnPropertyChanged(); } }
    private bool _verifyIntegrity;
    public bool VerifyIntegrity { get => _verifyIntegrity; set { _verifyIntegrity = value; OnPropertyChanged(); } }

    public IReadOnlyList<string> FrequencyOptions { get; } = new[] { "Daily", "Weekly", "Monthly" };

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad(); ApplyFrom(model); _snapshot = BuildModel(); EndLoad();
    }

    private void ApplyFrom(SettingsModel model)
    {
        var b = model.Backup;
        AutomaticBackup = b.AutomaticBackup; Frequency = b.Frequency; Time = b.Time;
        MaximumCount = b.MaximumCount; Compress = b.Compress; VerifyIntegrity = b.VerifyIntegrity;
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            Backup = new BackupSettingsModel
            {
                AutomaticBackup = AutomaticBackup, Frequency = Frequency, Time = Time,
                MaximumCount = MaximumCount, Compress = Compress, VerifyIntegrity = VerifyIntegrity
            }
        };
    }

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdateBackupSettingsCommand(BuildModel().Backup));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save backup settings."); return; }
        _snapshot = BuildModel(); IsDirty = false;
        ShowSuccess("Backup settings saved.");
    }

    public override async Task RestoreDefaultsAsync()
    {
        var result = await Mediator.Send(new ResetSettingsCommand());
        if (result.IsFailure) { ShowError(result.Error ?? "Could not reset settings."); return; }
        Load(result.Value);
        RequestGlobalReload?.Invoke();
        ShowSuccess("Settings restored to defaults.");
    }
}
