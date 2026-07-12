using MediatR;
using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Exceptions;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.Contracts;
using RMS.Modules.Customers.Domain.Entities;
using RMS.Modules.Customers.Domain.ValueObjects;

namespace RMS.Modules.Customers.Application.DeactivateCustomer;

public sealed class DeactivateCustomerHandler : IRequestHandler<DeactivateCustomerCommand, Result>
{
    private readonly ICustomerReadStore _readStore;
    private readonly ICustomerWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public DeactivateCustomerHandler(ICustomerReadStore readStore, ICustomerWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeactivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerModel = await _readStore.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customerModel is null)
            return Result.Failure("Customer not found.", "Customers.NotFound");

        var customer = Rehydrate(customerModel);

        try
        {
            customer.Deactivate();
            await _writeStore.UpdateAsync(customer, cancellationToken);
            await _eventBus.PublishAsync(
                new CustomerDeactivatedIntegrationEvent(customer.Id, customer.CustomerCode),
                cancellationToken);
            customer.ClearDomainEvents();
            return Result.Success();
        }
        catch (BusinessRuleValidationException ex)
        {
            return Result.Failure(ex.Message, ex.RuleName);
        }
    }

    private static Customer Rehydrate(CustomerReadModel model)
    {
        return Customer.Rehydrate(
            model.Id, model.CustomerCode, model.FirstName, model.LastName,
            PhoneNumber.Create(model.PhoneNumber),
            string.IsNullOrWhiteSpace(model.Email) ? null : Email.Create(model.Email),
            (string.IsNullOrWhiteSpace(model.Street) || string.IsNullOrWhiteSpace(model.City))
                ? null
                : Address.Create(model.Street, model.City, model.PostalCode, model.Country),
            Enum.Parse<CustomerStatus>(model.Status), model.CreatedAt, model.UpdatedAt);
    }
}

public sealed record CustomerDeactivatedIntegrationEvent(Guid CustomerId, string CustomerCode) : DomainEvent, IIntegrationEvent;
