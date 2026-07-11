using System.Collections.ObjectModel;
using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.Modules.Settings.Application.ResetSettings;
using RMS.Modules.Settings.Application.UpdateStorageSettings;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

public sealed class StorageSettingsViewModel : SettingsSectionViewModelBase
{
    private SettingsModel _snapshot = new();

    public StorageSettingsViewModel(
        IMediator mediator,
        IDialogService dialogService,
        IFolderBrowserService folderBrowser) : base(mediator, dialogService)
    {
        FolderBrowser = folderBrowser;
    }

    private IFolderBrowserService FolderBrowser { get; }

    public override string Title => "File Storage";
    public override string Description => "Where documents, exports and the database are stored.";

    public ObservableCollection<FolderSettingRowViewModel> Folders { get; } = new();

    protected override SettingsModel Snapshot { get => _snapshot; set => _snapshot = value; }

    public override void Load(SettingsModel model)
    {
        BeginLoad();
        Folders.Clear();
        foreach (var folder in model.Storage)
        {
            Folders.Add(new FolderSettingRowViewModel(FolderBrowser, DialogService)
            {
                Key = folder.Key,
                DisplayName = folder.DisplayName,
                Path = folder.Path,
                DefaultPath = folder.DefaultPath
            });
        }
        _snapshot = BuildModel();
        EndLoad();
    }

    private SettingsModel BuildModel()
    {
        return new SettingsModel
        {
            Storage = Folders.Select(f => f.ToModel()).ToList()
        };
    }

    public override void Cancel()
    {
        BeginLoad();
        Folders.Clear();
        foreach (var folder in _snapshot.Storage)
        {
            Folders.Add(new FolderSettingRowViewModel(FolderBrowser, DialogService)
            {
                Key = folder.Key,
                DisplayName = folder.DisplayName,
                Path = folder.Path,
                DefaultPath = folder.DefaultPath
            });
        }
        EndLoad();
    }

    public override async Task SaveAsync()
    {
        var result = await Mediator.Send(new UpdateStorageSettingsCommand(Folders.Select(f => f.ToModel()).ToList()));
        if (result.IsFailure) { ShowError(result.Error ?? "Could not save storage settings."); return; }
        _snapshot = BuildModel();
        IsDirty = false;
        ShowSuccess("Storage settings saved.");
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
