using MediatR;
using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Exceptions;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;
using RMS.Modules.Customers.Domain.Entities;
using RMS.Modules.Customers.Domain.ValueObjects;

namespace RMS.Modules.Customers.Application.UpdateCustomer;

public sealed class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly ICustomerReadStore _readStore;
    private readonly ICustomerWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdateCustomerHandler(ICustomerReadStore readStore, ICustomerWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerModel = await _readStore.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customerModel is null)
            return Result.Failure("Customer not found.", "Customers.NotFound");

        var existingByPhone = await _readStore.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        if (existingByPhone is not null && existingByPhone.Id != request.CustomerId)
            return Result.Failure("A customer with this phone number already exists.", "Customers.PhoneNumberAlreadyExists");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existingByEmail = await _readStore.GetByEmailAsync(request.Email, cancellationToken);
            if (existingByEmail is not null && existingByEmail.Id != request.CustomerId)
                return Result.Failure("A customer with this email already exists.", "Customers.EmailAlreadyExists");
        }

        var phone = PhoneNumber.Create(request.PhoneNumber);
        Email? email = string.IsNullOrWhiteSpace(request.Email) ? null : Email.Create(request.Email);
        Address? address = (string.IsNullOrWhiteSpace(request.Street) || string.IsNullOrWhiteSpace(request.City))
            ? null
            : Address.Create(request.Street, request.City, request.PostalCode, request.Country);

        var customer = Customer.Rehydrate(
            customerModel.Id, customerModel.CustomerCode, customerModel.FirstName, customerModel.LastName,
            PhoneNumber.Create(customerModel.PhoneNumber),
            string.IsNullOrWhiteSpace(customerModel.Email) ? null : Email.Create(customerModel.Email),
            (string.IsNullOrWhiteSpace(customerModel.Street) || string.IsNullOrWhiteSpace(customerModel.City))
                ? null
                : Address.Create(customerModel.Street, customerModel.City, customerModel.PostalCode, customerModel.Country),
            Enum.Parse<CustomerStatus>(customerModel.Status), customerModel.CreatedAt, customerModel.UpdatedAt);

        try
        {
            customer.Update(request.FirstName, request.LastName, phone, email, address);
            await _writeStore.UpdateAsync(customer, cancellationToken);
            customer.ClearDomainEvents();
            await _eventBus.PublishAsync(new CustomerUpdatedIntegrationEvent(customer.Id, customer.CustomerCode, customer.FullName), cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleValidationException ex)
        {
            return Result.Failure(ex.Message, ex.RuleName);
        }
    }
}

public sealed record CustomerUpdatedIntegrationEvent(Guid CustomerId, string CustomerCode, string FullName) : DomainEvent, IIntegrationEvent;
