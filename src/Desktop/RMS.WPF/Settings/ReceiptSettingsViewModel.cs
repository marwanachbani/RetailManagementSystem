using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateReceiptSettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class ReceiptSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public ReceiptSettingsViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService) { }

    public override string Title => "Receipts";
    public override string Description => "What is printed on every customer receipt.";

    private string _header = string.Empty;
    public string Header { get => _header; set { _header = value; OnPropertyChanged(); } }
    private string _footer = string.Empty;
    public string Footer { get => _footer; set { _footer = value; OnPropertyChanged(); } }
    private string _storeLogo = string.Empty;
    public string StoreLogo { get => _storeLogo; set { _storeLogo = value; OnPropertyChanged(); } }
    private bool _showTaxNumber;
    public bool ShowTaxNumber { get => _showTaxNumber; set { _showTaxNumber = value; OnPropertyChanged(); } }
    private bool _showCashier;
    public bool ShowCashier { get => _showCashier; set { _showCashier = value; OnPropertyChanged(); } }
    private bool _showBarcode;
    public bool ShowBarcode { get => _showBarcode; set { _showBarcode = value; OnPropertyChanged(); } }
    private bool _showQrCode;
    public bool ShowQrCode { get => _showQrCode; set { _showQrCode = value; OnPropertyChanged(); } }
    private int _paperWidth;
    public int PaperWidth { get => _paperWidth; set { _paperWidth = value; OnPropertyChanged(); } }
    private bool _automaticPrinting;
    public bool AutomaticPrinting { get => _automaticPrinting; set { _automaticPrinting = value; OnPropertyChanged(); } }

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad(); ApplyFrom(model); _snapshot = BuildModel(); EndLoad();
    }

    private void ApplyFrom(SettingsModel model)
    {
        var r = model.Receipt;
        Header = r.Header; Footer = r.Footer; StoreLogo = r.StoreLogo;
        ShowTaxNumber = r.ShowTaxNumber; ShowCashier = r.ShowCashier;
        ShowBarcode = r.ShowBarcode; ShowQrCode = r.ShowQrCode;
        PaperWidth = r.PaperWidth; AutomaticPrinting = r.AutomaticPrinting;
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            Receipt = new ReceiptSettingsModel
            {
                Header = Header, Footer = Footer, StoreLogo = StoreLogo,
                ShowTaxNumber = ShowTaxNumber, ShowCashier = ShowCashier,
                ShowBarcode = ShowBarcode, ShowQrCode = ShowQrCode,
                PaperWidth = PaperWidth, AutomaticPrinting = AutomaticPrinting
            }
        };
    }

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdateReceiptSettingsCommand(BuildModel().Receipt));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save receipt settings."); return; }
        _snapshot = BuildModel(); IsDirty = false;
        ShowSuccess("Receipt settings saved.");
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
