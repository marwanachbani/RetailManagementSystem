using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateStoreSettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class StoreSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public StoreSettingsViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService) { }

    public override string Title => "Store";
    public override string Description => "Logo and postal address of the business.";

    private string _businessLogo = string.Empty;
    public string BusinessLogo { get => _businessLogo; set { _businessLogo = value; OnPropertyChanged(); } }
    private string _companyAddress = string.Empty;
    public string CompanyAddress { get => _companyAddress; set { _companyAddress = value; OnPropertyChanged(); } }

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad();
        ApplyFrom(model);
        _snapshot = BuildModel();
        EndLoad();
    }

    private void ApplyFrom(SettingsModel model)
    {
        BusinessLogo = model.Store.BusinessLogo;
        CompanyAddress = model.Store.CompanyAddress;
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            Store = new StoreSettingsModel { BusinessLogo = BusinessLogo, CompanyAddress = CompanyAddress }
        };
    }

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdateStoreSettingsCommand(BuildModel().Store));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save store settings."); return; }
        _snapshot = BuildModel();
        IsDirty = false;
        ShowSuccess("Store settings saved.");
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
