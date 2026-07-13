using System.Collections.ObjectModel;
using System.Windows.Input;
using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Settings.Application.GetSettings;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.Commands;
using RMS.WPF.Services;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Settings;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private readonly IPrintingService _printing;
    private SettingsSectionViewModelBase _selectedSection;
    private bool _isLoading;

    public SettingsViewModel(
        IMediator mediator,
        IDialogService dialogService,
        IPrintingService printing,
        IFolderBrowserService folderBrowser)
    {
        _mediator = mediator;
        _dialogService = dialogService;
        _printing = printing;

        Sections = new ObservableCollection<SettingsSectionViewModelBase>
        {
            new GeneralSettingsViewModel(_mediator, _dialogService),
            new StoreSettingsViewModel(_mediator, _dialogService),
            new ReceiptSettingsViewModel(_mediator, _dialogService),
            new SalesSettingsViewModel(_mediator, _dialogService),
            new InventorySettingsViewModel(_mediator, _dialogService),
            new PurchasingSettingsViewModel(_mediator, _dialogService),
            new ReportSettingsViewModel(_mediator, _dialogService),
            new StorageSettingsViewModel(_mediator, _dialogService, folderBrowser),
            new BackupSettingsViewModel(_mediator, _dialogService),
            new AppearanceSettingsViewModel(_mediator, _dialogService),
            new PrinterSettingsViewModel(_mediator, _dialogService, _printing)
        };

        _selectedSection = Sections[0];
        foreach (var section in Sections)
            section.RequestGlobalReload = () => _ = LoadAsync();

        SelectSectionCommand = new RelayCommand(p => SelectedSection = (SettingsSectionViewModelBase)p!);
    }

    public ObservableCollection<SettingsSectionViewModelBase> Sections { get; }

    public SettingsSectionViewModelBase SelectedSection
    {
        get => _selectedSection;
        set { _selectedSection = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand SelectSectionCommand { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _mediator.Send(new GetSettingsQuery());
            if (result.IsFailure)
            {
                _dialogService.ShowError(result.Error ?? "Could not load settings.");
                return;
            }

            foreach (var section in Sections)
                section.Load(result.Value);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
