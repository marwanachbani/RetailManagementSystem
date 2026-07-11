using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateInventorySettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class InventorySettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public InventorySettingsViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService) { }

    public override string Title => "Inventory";
    public override string Description => "Stock thresholds and adjustment rules.";

    private int _defaultLowStockThreshold;
    public int DefaultLowStockThreshold { get => _defaultLowStockThreshold; set { _defaultLowStockThreshold = value; OnPropertyChanged(); } }
    private bool _allowNegativeStock;
    public bool AllowNegativeStock { get => _allowNegativeStock; set { _allowNegativeStock = value; OnPropertyChanged(); } }
    private bool _automaticStockAlerts;
    public bool AutomaticStockAlerts { get => _automaticStockAlerts; set { _automaticStockAlerts = value; OnPropertyChanged(); } }
    private bool _requireConfirmationForAdjustments;
    public bool RequireConfirmationForAdjustments { get => _requireConfirmationForAdjustments; set { _requireConfirmationForAdjustments = value; OnPropertyChanged(); } }
    private string _defaultWarehouse = string.Empty;
    public string DefaultWarehouse { get => _defaultWarehouse; set { _defaultWarehouse = value; OnPropertyChanged(); } }
    private string _defaultStockAdjustmentReason = string.Empty;
    public string DefaultStockAdjustmentReason { get => _defaultStockAdjustmentReason; set { _defaultStockAdjustmentReason = value; OnPropertyChanged(); } }

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad(); ApplyFrom(model); _snapshot = BuildModel(); EndLoad();
    }

    private void ApplyFrom(SettingsModel model)
    {
        var i = model.Inventory;
        DefaultLowStockThreshold = i.DefaultLowStockThreshold; AllowNegativeStock = i.AllowNegativeStock;
        AutomaticStockAlerts = i.AutomaticStockAlerts; RequireConfirmationForAdjustments = i.RequireConfirmationForAdjustments;
        DefaultWarehouse = i.DefaultWarehouse; DefaultStockAdjustmentReason = i.DefaultStockAdjustmentReason;
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            Inventory = new InventorySettingsModel
            {
                DefaultLowStockThreshold = DefaultLowStockThreshold, AllowNegativeStock = AllowNegativeStock,
                AutomaticStockAlerts = AutomaticStockAlerts, RequireConfirmationForAdjustments = RequireConfirmationForAdjustments,
                DefaultWarehouse = DefaultWarehouse, DefaultStockAdjustmentReason = DefaultStockAdjustmentReason
            }
        };
    }

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdateInventorySettingsCommand(BuildModel().Inventory));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save inventory settings."); return; }
        _snapshot = BuildModel(); IsDirty = false;
        ShowSuccess("Inventory settings saved.");
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
