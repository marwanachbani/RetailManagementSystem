using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateSalesSettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class SalesSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public SalesSettingsViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService) { }

    public override string Title => "Sales";
    public override string Description => "Taxes, discounts and sale behavior.";

    private decimal _defaultTaxRate;
    public decimal DefaultTaxRate { get => _defaultTaxRate; set { _defaultTaxRate = value; OnPropertyChanged(); } }
    private decimal _defaultDiscount;
    public decimal DefaultDiscount { get => _defaultDiscount; set { _defaultDiscount = value; OnPropertyChanged(); } }
    private decimal _maximumDiscount;
    public decimal MaximumDiscount { get => _maximumDiscount; set { _maximumDiscount = value; OnPropertyChanged(); } }
    private bool _allowManualPriceChange;
    public bool AllowManualPriceChange { get => _allowManualPriceChange; set { _allowManualPriceChange = value; OnPropertyChanged(); } }
    private bool _requireManagerApproval;
    public bool RequireManagerApproval { get => _requireManagerApproval; set { _requireManagerApproval = value; OnPropertyChanged(); } }
    private string _defaultPaymentMethod = string.Empty;
    public string DefaultPaymentMethod { get => _defaultPaymentMethod; set { _defaultPaymentMethod = value; OnPropertyChanged(); } }
    private bool _autoCompleteSale;
    public bool AutoCompleteSale { get => _autoCompleteSale; set { _autoCompleteSale = value; OnPropertyChanged(); } }
    private bool _receiptPreview;
    public bool ReceiptPreview { get => _receiptPreview; set { _receiptPreview = value; OnPropertyChanged(); } }

    public IReadOnlyList<string> PaymentMethods { get; } = new[] { "Cash", "Card", "Bank Transfer", "Credit" };

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad(); ApplyFrom(model); _snapshot = BuildModel(); EndLoad();
    }

    private void ApplyFrom(SettingsModel model)
    {
        var s = model.Sales;
        DefaultTaxRate = s.DefaultTaxRate; DefaultDiscount = s.DefaultDiscount;
        MaximumDiscount = s.MaximumDiscount; AllowManualPriceChange = s.AllowManualPriceChange;
        RequireManagerApproval = s.RequireManagerApproval; DefaultPaymentMethod = s.DefaultPaymentMethod;
        AutoCompleteSale = s.AutoCompleteSale; ReceiptPreview = s.ReceiptPreview;
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            Sales = new SalesSettingsModel
            {
                DefaultTaxRate = DefaultTaxRate, DefaultDiscount = DefaultDiscount,
                MaximumDiscount = MaximumDiscount, AllowManualPriceChange = AllowManualPriceChange,
                RequireManagerApproval = RequireManagerApproval, DefaultPaymentMethod = DefaultPaymentMethod,
                AutoCompleteSale = AutoCompleteSale, ReceiptPreview = ReceiptPreview
            }
        };
    }

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdateSalesSettingsCommand(BuildModel().Sales));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save sales settings."); return; }
        _snapshot = BuildModel(); IsDirty = false;
        ShowSuccess("Sales settings saved.");
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
