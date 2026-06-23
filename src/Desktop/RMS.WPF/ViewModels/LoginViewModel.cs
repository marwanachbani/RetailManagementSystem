using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Identity.Application.AuthenticateUser;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

/// <summary>
/// ViewModel for the login screen. Follows MVVM: no WPF-specific logic,
/// no direct UI manipulation — just commands, properties, and events.
/// </summary>
public sealed class LoginViewModel : INotifyPropertyChanged
{
    private readonly IMediator _mediator;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string? _errorMessage;
    private bool _isLoading;

    public LoginViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoginCommand = new RelayCommand(_ => _ = LoginAsync(), _ => !IsLoading && !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password));
    }

    public string UserName
    {
        get => _userName;
        set
        {
            _userName = value;
            OnPropertyChanged();
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Bound from the code-behind (PasswordBox does not support direct binding for security).
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            RaiseCanExecuteChanged();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            _isLoading = value;
            OnPropertyChanged();
            RaiseCanExecuteChanged();
        }
    }

    public ICommand LoginCommand { get; }

    /// <summary>
    /// Raised when authentication succeeds. The App or Window subscribes
    /// and transitions to the main shell.
    /// </summary>
    public event EventHandler<AuthenticateUserResult>? LoginSucceeded;

    private async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var query = new AuthenticateUserQuery(UserName.Trim(), Password);
            Result<AuthenticateUserResult> result = await _mediator.Send(query);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error ?? "Authentication failed.";
                return;
            }

            LoginSucceeded?.Invoke(this, result.Value);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void RaiseCanExecuteChanged()
    {
        // Force re-evaluation of the LoginCommand's CanExecute.
        CommandManager.InvalidateRequerySuggested();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
