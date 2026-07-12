using MediatR;
using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;
using RMS.Modules.Customers.Domain.Entities;
using RMS.Modules.Customers.Domain.ValueObjects;

namespace RMS.Modules.Customers.Application.CreateCustomer;

public sealed class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly ICustomerReadStore _readStore;
    private readonly ICustomerWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public CreateCustomerHandler(ICustomerReadStore readStore, ICustomerWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
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

        var customer = Customer.Create(
            Guid.NewGuid(),
            request.FirstName,
            request.LastName,
            PhoneNumber.Create(request.PhoneNumber),
            string.IsNullOrWhiteSpace(request.Email) ? null : Email.Create(request.Email),
            (string.IsNullOrWhiteSpace(request.Street) || string.IsNullOrWhiteSpace(request.City))
                ? null
                : Address.Create(request.Street, request.City, request.PostalCode, request.Country));

        await _writeStore.InsertAsync(customer, cancellationToken);
        customer.ClearDomainEvents();
        await _eventBus.PublishAsync(new CustomerCreatedIntegrationEvent(customer.Id, customer.CustomerCode, customer.FullName, customer.PhoneNumber.Value, customer.Email?.Value), cancellationToken);
        return Result.Success(customer.Id);
    }
}

public sealed record CustomerCreatedIntegrationEvent(
    Guid CustomerId,
    string CustomerCode,
    string FullName,
    string PhoneNumber,
    string? Email) : DomainEvent, IIntegrationEvent;
