using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Identity.Application.IntegrationEvents;
using RMS.Modules.Identity.Application.AuthenticateUser;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class LoginViewModel : INotifyPropertyChanged
{
    private readonly IMediator _mediator;
    private readonly IEventBus _eventBus;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string? _errorMessage;
    private bool _isLoading;
    private bool _showPassword;
    private bool _rememberMe;

    public LoginViewModel(IMediator mediator, IEventBus eventBus)
    {
        _mediator = mediator;
        _eventBus = eventBus;
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

    public bool ShowPassword
    {
        get => _showPassword;
        set
        {
            _showPassword = value;
            OnPropertyChanged();
        }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set
        {
            _rememberMe = value;
            OnPropertyChanged();
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
                await _eventBus.PublishAsync(new LoginFailedIntegrationEvent(UserName.Trim(), result.Error ?? "Authentication failed."), default);
                return;
            }

            await _eventBus.PublishAsync(new LoginSucceededIntegrationEvent(result.Value.UserId, result.Value.UserName), default);
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
        CommandManager.InvalidateRequerySuggested();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
