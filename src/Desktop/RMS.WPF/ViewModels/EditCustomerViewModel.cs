using System.Windows;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Customers.Application.Contracts;
using RMS.Modules.Customers.Application.UpdateCustomer;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class EditCustomerViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private Guid _customerId;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _phoneNumber = string.Empty;
    private string? _email;
    private string? _street;
    private string? _city;
    private string? _postalCode;
    private string? _country;
    private string? _statusMessage;
    private bool _isBusy;

    public EditCustomerViewModel(IMediator mediator)
    {
        _mediator = mediator;
        SaveCommand = new RelayCommand(_ => _ = SaveAsync(), _ => CanSave);
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, false));
    }

    public void LoadCustomer(CustomerReadModel customer)
    {
        _customerId = customer.Id;
        FirstName = customer.FirstName;
        LastName = customer.LastName;
        PhoneNumber = customer.PhoneNumber;
        Email = customer.Email;
        Street = customer.Street;
        City = customer.City;
        PostalCode = customer.PostalCode;
        Country = customer.Country;
        StatusMessage = null;
    }

    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged();
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged();
        }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged();
        }
    }

    public string? Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
        }
    }

    public string? Street
    {
        get => _street;
        set
        {
            _street = value;
            OnPropertyChanged();
        }
    }

    public string? City
    {
        get => _city;
        set
        {
            _city = value;
            OnPropertyChanged();
        }
    }

    public string? PostalCode
    {
        get => _postalCode;
        set
        {
            _postalCode = value;
            OnPropertyChanged();
        }
    }

    public string? Country
    {
        get => _country;
        set
        {
            _country = value;
            OnPropertyChanged();
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    private bool CanSave => !string.IsNullOrWhiteSpace(FirstName)
                         && !string.IsNullOrWhiteSpace(LastName)
                         && !string.IsNullOrWhiteSpace(PhoneNumber);

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler<bool>? CloseRequested;

    public async Task SaveAsync()
    {
        IsBusy = true;
        StatusMessage = null;

        var command = new UpdateCustomerCommand(
            _customerId, FirstName, LastName, PhoneNumber, Email, Street, City, PostalCode, Country);

        var result = await _mediator.Send(command);
        IsBusy = false;

        if (result.IsSuccess)
        {
            CloseRequested?.Invoke(this, true);
        }
        else
        {
            StatusMessage = result.Error;
        }
    }
}
