using System.Windows;
using System.Windows.Input;
using MediatR;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.Modules.Suppliers.Application.UpdateSupplier;
using RMS.WPF.Commands;

namespace RMS.WPF.ViewModels;

public sealed class EditSupplierViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private Guid _supplierId;
    private string _companyName = string.Empty;
    private string _phoneNumber = string.Empty;
    private string? _contactPerson;
    private string? _email;
    private string? _vatNumber;
    private string? _street;
    private string? _city;
    private string? _postalCode;
    private string? _country;
    private string? _statusMessage;
    private bool _isBusy;

    public EditSupplierViewModel(IMediator mediator)
    {
        _mediator = mediator;
        SaveCommand = new RelayCommand(_ => _ = SaveAsync(), _ => CanSave);
        CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, false));
    }

    public void LoadSupplier(SupplierReadModel supplier)
    {
        _supplierId = supplier.Id;
        CompanyName = supplier.CompanyName;
        PhoneNumber = supplier.PhoneNumber;
        ContactPerson = supplier.ContactPerson;
        Email = supplier.Email;
        VatNumber = supplier.VatNumber;
        Street = supplier.Street;
        City = supplier.City;
        PostalCode = supplier.PostalCode;
        Country = supplier.Country;
        StatusMessage = null;
    }

    public string CompanyName
    {
        get => _companyName;
        set
        {
            _companyName = value;
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

    public string? ContactPerson
    {
        get => _contactPerson;
        set
        {
            _contactPerson = value;
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

    public string? VatNumber
    {
        get => _vatNumber;
        set
        {
            _vatNumber = value;
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

    private bool CanSave => !string.IsNullOrWhiteSpace(CompanyName)
                         && !string.IsNullOrWhiteSpace(PhoneNumber);

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler<bool>? CloseRequested;

    public async Task SaveAsync()
    {
        IsBusy = true;
        StatusMessage = null;

        var command = new UpdateSupplierCommand(
            _supplierId, CompanyName, PhoneNumber, ContactPerson, Email, VatNumber, Street, City, PostalCode, Country);

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
