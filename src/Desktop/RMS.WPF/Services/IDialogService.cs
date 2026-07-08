using System.Windows;

namespace RMS.WPF.Services;

/// <summary>
/// Small abstraction over MessageBox so view models can ask for confirmation
/// before destructive actions (delete) and surface validation errors as a
/// popup instead of silently failing or only updating a status label.
/// </summary>
public interface IDialogService
{
    bool Confirm(string message, string title = "Please Confirm");
    void ShowError(string message, string title = "Error");
    void ShowWarning(string message, string title = "Warning");
    void ShowInfo(string message, string title = "Information");
}

public sealed class DialogService : IDialogService
{
    public bool Confirm(string message, string title = "Please Confirm")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
        return result == MessageBoxResult.Yes;
    }

    public void ShowError(string message, string title = "Error")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public void ShowWarning(string message, string title = "Warning")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public void ShowInfo(string message, string title = "Information")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
}
