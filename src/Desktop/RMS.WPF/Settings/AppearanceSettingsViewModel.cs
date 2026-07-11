using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateApplicationSettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class AppearanceSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public AppearanceSettingsViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService) { }

    public override string Title => "Appearance";
    public override string Description => "Theme, startup page and session behavior.";

    private string _theme = string.Empty;
    public string Theme { get => _theme; set { _theme = value; OnPropertyChanged(); } }
    private string _startupPage = string.Empty;
    public string StartupPage { get => _startupPage; set { _startupPage = value; OnPropertyChanged(); } }
    private bool _rememberLastUser;
    public bool RememberLastUser { get => _rememberLastUser; set { _rememberLastUser = value; OnPropertyChanged(); } }
    private bool _autoSave;
    public bool AutoSave { get => _autoSave; set { _autoSave = value; OnPropertyChanged(); } }
    private int _sessionTimeout;
    public int SessionTimeout { get => _sessionTimeout; set { _sessionTimeout = value; OnPropertyChanged(); } }

    public IReadOnlyList<string> ThemeOptions { get; } = new[] { "Light", "Dark" };
    public IReadOnlyList<string> StartupPageOptions { get; } = new[] { "Dashboard", "Sales", "Inventory", "Products" };

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad(); ApplyFrom(model); _snapshot = BuildModel(); EndLoad();
    }

    private void ApplyFrom(SettingsModel model)
    {
        var a = model.Application;
        Theme = a.Theme; StartupPage = a.StartupPage; RememberLastUser = a.RememberLastUser;
        AutoSave = a.AutoSave; SessionTimeout = a.SessionTimeout;
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            Application = new ApplicationSettingsModel
            {
                Theme = Theme, StartupPage = StartupPage, RememberLastUser = RememberLastUser,
                AutoSave = AutoSave, SessionTimeout = SessionTimeout
            }
        };
    }

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdateApplicationSettingsCommand(BuildModel().Application));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save appearance settings."); return; }
        _snapshot = BuildModel(); IsDirty = false;
        ShowSuccess("Appearance settings saved.");
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
