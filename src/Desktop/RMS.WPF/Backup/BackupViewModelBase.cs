using RMS.WPF.ViewModels;

namespace RMS.WPF.Backup;

/// <summary>The sub-screens of the Backup &amp; Restore module, used for in-module navigation.</summary>
public enum BackupScreen
{
    Dashboard,
    Manual,
    Restore,
    History,
    Settings
}

/// <summary>Base for the Backup &amp; Restore view models. Carries the in-module navigation delegate.</summary>
public abstract class BackupViewModelBase : ViewModelBase
{
    public Action<BackupScreen>? Navigate { get; set; }
}
