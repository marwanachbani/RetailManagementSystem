using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateReportSettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class ReportSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public ReportSettingsViewModel(IMediator mediator, IDialogService dialogService) : base(mediator, dialogService) { }

    public override string Title => "Reports";
    public override string Description => "Export destinations and report formatting.";

    private string _defaultReportFolder = string.Empty;
    public string DefaultReportFolder { get => _defaultReportFolder; set { _defaultReportFolder = value; OnPropertyChanged(); } }
    private string _fileNamePattern = string.Empty;
    public string FileNamePattern { get => _fileNamePattern; set { _fileNamePattern = value; OnPropertyChanged(); } }
    private string _pdfQuality = string.Empty;
    public string PdfQuality { get => _pdfQuality; set { _pdfQuality = value; OnPropertyChanged(); } }
    private string _csvDelimiter = string.Empty;
    public string CsvDelimiter { get => _csvDelimiter; set { _csvDelimiter = value; OnPropertyChanged(); } }
    private string _excelExportFormat = string.Empty;
    public string ExcelExportFormat { get => _excelExportFormat; set { _excelExportFormat = value; OnPropertyChanged(); } }
    private string _printOrientation = string.Empty;
    public string PrintOrientation { get => _printOrientation; set { _printOrientation = value; OnPropertyChanged(); } }
    private bool _includeCompanyLogo;
    public bool IncludeCompanyLogo { get => _includeCompanyLogo; set { _includeCompanyLogo = value; OnPropertyChanged(); } }
    private bool _includeReportFooter;
    public bool IncludeReportFooter { get => _includeReportFooter; set { _includeReportFooter = value; OnPropertyChanged(); } }
    private bool _automaticNumbering;
    public bool AutomaticNumbering { get => _automaticNumbering; set { _automaticNumbering = value; OnPropertyChanged(); } }

    public IReadOnlyList<string> PdfQualityOptions { get; } = new[] { "Draft", "Standard", "High" };
    public IReadOnlyList<string> CsvDelimiterOptions { get; } = new[] { ",", ";", "\\t", "|" };
    public IReadOnlyList<string> ExcelExportFormatOptions { get; } = new[] { "Xlsx", "Xls", "Csv" };
    public IReadOnlyList<string> PrintOrientationOptions { get; } = new[] { "Portrait", "Landscape" };

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad(); ApplyFrom(model); _snapshot = BuildModel(); EndLoad();
    }

    private void ApplyFrom(SettingsModel model)
    {
        var r = model.Report;
        DefaultReportFolder = r.DefaultReportFolder; FileNamePattern = r.FileNamePattern;
        PdfQuality = r.PdfQuality; CsvDelimiter = r.CsvDelimiter;
        ExcelExportFormat = r.ExcelExportFormat; PrintOrientation = r.PrintOrientation;
        IncludeCompanyLogo = r.IncludeCompanyLogo; IncludeReportFooter = r.IncludeReportFooter;
        AutomaticNumbering = r.AutomaticNumbering;
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            Report = new ReportSettingsModel
            {
                DefaultReportFolder = DefaultReportFolder, FileNamePattern = FileNamePattern,
                PdfQuality = PdfQuality, CsvDelimiter = CsvDelimiter,
                ExcelExportFormat = ExcelExportFormat, PrintOrientation = PrintOrientation,
                IncludeCompanyLogo = IncludeCompanyLogo, IncludeReportFooter = IncludeReportFooter,
                AutomaticNumbering = AutomaticNumbering
            }
        };
    }

    public override void Cancel() { BeginLoad(); ApplyFrom(_snapshot); EndLoad(); }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdateReportSettingsCommand(BuildModel().Report));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save report settings."); return; }
        _snapshot = BuildModel(); IsDirty = false;
        ShowSuccess("Report settings saved.");
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
