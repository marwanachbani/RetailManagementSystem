using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;
using RMS.Modules.Customers.Domain.Entities;
using RMS.Modules.Customers.Domain.ValueObjects;

namespace RMS.Modules.Customers.Application.CreateCustomer;

public sealed class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly ICustomerReadStore _readStore;
    private readonly ICustomerWriteStore _writeStore;

    public CreateCustomerHandler(ICustomerReadStore readStore, ICustomerWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existingByPhone = await _readStore.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        if (existingByPhone is not null)
            return Result.Failure<Guid>("A customer with this phone number already exists.", "Customers.PhoneNumberAlreadyExists");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existingByEmail = await _readStore.GetByEmailAsync(request.Email, cancellationToken);
            if (existingByEmail is not null)
                return Result.Failure<Guid>("A customer with this email already exists.", "Customers.EmailAlreadyExists");
        }

        var phone = PhoneNumber.Create(request.PhoneNumber);
        Email? email = string.IsNullOrWhiteSpace(request.Email) ? null : Email.Create(request.Email);
        Address? address = (string.IsNullOrWhiteSpace(request.Street) || string.IsNullOrWhiteSpace(request.City))
            ? null
            : Address.Create(request.Street, request.City, request.PostalCode, request.Country);

        var customer = Customer.Create(Guid.NewGuid(), request.FirstName, request.LastName, phone, email, address);

        await _writeStore.InsertAsync(customer, cancellationToken);
        customer.ClearDomainEvents();

        return Result.Success(customer.Id);
    }
}
