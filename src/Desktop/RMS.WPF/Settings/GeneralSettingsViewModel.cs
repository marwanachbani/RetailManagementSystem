using System.Windows.Input;
using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.GetSettings;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateGeneralSettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class GeneralSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public GeneralSettingsViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService) { }

    public override string Title => "General";
    public override string Description => "Business identity and localization.";

    private string _storeName = string.Empty;
    public string StoreName { get => _storeName; set { _storeName = value; OnPropertyChanged(); } }
    private string _phoneNumber = string.Empty;
    public string PhoneNumber { get => _phoneNumber; set { _phoneNumber = value; OnPropertyChanged(); } }
    private string _email = string.Empty;
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    private string _website = string.Empty;
    public string Website { get => _website; set { _website = value; OnPropertyChanged(); } }
    private string _taxNumber = string.Empty;
    public string TaxNumber { get => _taxNumber; set { _taxNumber = value; OnPropertyChanged(); } }
    private string _currency = string.Empty;
    public string Currency { get => _currency; set { _currency = value; OnPropertyChanged(); } }
    private string _timeZone = string.Empty;
    public string TimeZone { get => _timeZone; set { _timeZone = value; OnPropertyChanged(); } }
    private string _language = string.Empty;
    public string Language { get => _language; set { _language = value; OnPropertyChanged(); } }
    private string _dateFormat = string.Empty;
    public string DateFormat { get => _dateFormat; set { _dateFormat = value; OnPropertyChanged(); } }
    private string _timeFormat = string.Empty;
    public string TimeFormat { get => _timeFormat; set { _timeFormat = value; OnPropertyChanged(); } }
    private string _numberFormat = string.Empty;
    public string NumberFormat { get => _numberFormat; set { _numberFormat = value; OnPropertyChanged(); } }

    public IReadOnlyList<string> CurrencyOptions { get; } = new[] { "USD", "EUR", "GBP", "JPY", "AUD", "CAD" };
    public IReadOnlyList<string> LanguageOptions { get; } = new[] { "English", "Spanish", "French", "German", "Arabic" };
    public IReadOnlyList<string> DateFormatOptions { get; } = new[] { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "dd.MM.yyyy" };
    public IReadOnlyList<string> TimeFormatOptions { get; } = new[] { "HH:mm", "hh:mm tt" };

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
        var g = model.General;
        StoreName = g.StoreName;
        PhoneNumber = g.PhoneNumber;
        Email = g.Email;
        Website = g.Website;
        TaxNumber = g.TaxNumber;
        Currency = g.Currency;
        TimeZone = g.TimeZone;
        Language = g.Language;
        DateFormat = g.DateFormat;
        TimeFormat = g.TimeFormat;
        NumberFormat = g.NumberFormat;
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            General = new GeneralSettingsModel
            {
                StoreName = StoreName,
                PhoneNumber = PhoneNumber,
                Email = Email,
                Website = Website,
                TaxNumber = TaxNumber,
                Currency = Currency,
                TimeZone = TimeZone,
                Language = Language,
                DateFormat = DateFormat,
                TimeFormat = TimeFormat,
                NumberFormat = NumberFormat
            }
        };
    }

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdateGeneralSettingsCommand(BuildModel().General));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save general settings."); return; }
        _snapshot = BuildModel();
        IsDirty = false;
        ShowSuccess("General settings saved.");
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
