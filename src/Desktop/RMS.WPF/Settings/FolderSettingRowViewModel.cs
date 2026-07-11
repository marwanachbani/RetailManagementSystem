using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using RMS.Modules.Settings.Application.Models;
using RMS.WPF.ViewModels;
using RMS.WPF.Commands;
using RMS.WPF.Services;

namespace RMS.WPF.Settings;

/// <summary>
/// View model for a single configurable folder (File Storage section). Exposes
/// Browse / Open / Restore Default actions and a live "exists" indicator.
/// </summary>
public sealed class FolderSettingRowViewModel : ViewModelBase
{
    private readonly IFolderBrowserService _folderBrowser;
    private string _path = string.Empty;
    private string _defaultPath = string.Empty;

    public FolderSettingRowViewModel(IFolderBrowserService folderBrowser, IDialogService dialogService)
    {
        _folderBrowser = folderBrowser;
        DialogService = dialogService;
        BrowseCommand = new RelayCommand(_ => Browse());
        OpenCommand = new RelayCommand(_ => Open());
        RestoreDefaultCommand = new RelayCommand(_ => RestoreDefault());
    }

    private IDialogService DialogService { get; }

    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public string Path
    {
        get => _path;
        set { _path = value; OnPropertyChanged(); OnPropertyChanged(nameof(Exists)); }
    }

    public string DefaultPath
    {
        get => _defaultPath;
        set { _defaultPath = value; OnPropertyChanged(); }
    }

    public bool Exists => !string.IsNullOrWhiteSpace(_path) && Directory.Exists(_path);

    public ICommand BrowseCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand RestoreDefaultCommand { get; }

    private void Browse()
    {
        var picked = _folderBrowser.Browse(_path, $"Select {DisplayName} folder");
        if (!string.IsNullOrWhiteSpace(picked))
            Path = picked;
    }

    private void Open()
    {
        if (string.IsNullOrWhiteSpace(_path)) return;
        if (!Directory.Exists(_path)) Directory.CreateDirectory(_path);
        Process.Start(new ProcessStartInfo
        {
            FileName = _path,
            UseShellExecute = true
        });
    }

    private void RestoreDefault()
    {
        if (string.IsNullOrWhiteSpace(_defaultPath)) return;
        if (DialogService.Confirm($"Restore the default path for '{DisplayName}'?", "Restore Default"))
            Path = _defaultPath;
    }

    public FolderSettingModel ToModel() => new() { Key = Key, DisplayName = DisplayName, Path = Path, DefaultPath = DefaultPath };
}
