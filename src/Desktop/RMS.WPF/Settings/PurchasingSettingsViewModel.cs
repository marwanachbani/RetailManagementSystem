using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdatePurchasingSettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class PurchasingSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public PurchasingSettingsViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService) { }

    public override string Title => "Purchasing";
    public override string Description => "Supplier defaults and purchase numbering.";

    private string _defaultSupplier = string.Empty;
    public string DefaultSupplier { get => _defaultSupplier; set { _defaultSupplier = value; OnPropertyChanged(); } }
    private string _purchaseNumberPrefix = string.Empty;
    public string PurchaseNumberPrefix { get => _purchaseNumberPrefix; set { _purchaseNumberPrefix = value; OnPropertyChanged(); } }
    private bool _automaticGoodsReceipt;
    public bool AutomaticGoodsReceipt { get => _automaticGoodsReceipt; set { _automaticGoodsReceipt = value; OnPropertyChanged(); } }
    private string _defaultPaymentTerms = string.Empty;
    public string DefaultPaymentTerms { get => _defaultPaymentTerms; set { _defaultPaymentTerms = value; OnPropertyChanged(); } }

    public IReadOnlyList<string> PaymentTermsOptions { get; } = new[] { "Net 15", "Net 30", "Net 60", "Due on Receipt" };

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad(); ApplyFrom(model); _snapshot = BuildModel(); EndLoad();
    }

    private void ApplyFrom(SettingsModel model)
    {
        var p = model.Purchasing;
        DefaultSupplier = p.DefaultSupplier; PurchaseNumberPrefix = p.PurchaseNumberPrefix;
        AutomaticGoodsReceipt = p.AutomaticGoodsReceipt; DefaultPaymentTerms = p.DefaultPaymentTerms;
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            Purchasing = new PurchasingSettingsModel
            {
                DefaultSupplier = DefaultSupplier, PurchaseNumberPrefix = PurchaseNumberPrefix,
                AutomaticGoodsReceipt = AutomaticGoodsReceipt, DefaultPaymentTerms = DefaultPaymentTerms
            }
        };
    }

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdatePurchasingSettingsCommand(BuildModel().Purchasing));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save purchasing settings."); return; }
        _snapshot = BuildModel(); IsDirty = false;
        ShowSuccess("Purchasing settings saved.");
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
