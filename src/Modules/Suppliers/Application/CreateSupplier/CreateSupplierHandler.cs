using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.Modules.Suppliers.Domain.Entities;
using RMS.Modules.Suppliers.Domain.ValueObjects;

namespace RMS.Modules.Suppliers.Application.CreateSupplier;

public sealed class CreateSupplierHandler : IRequestHandler<CreateSupplierCommand, Result<Guid>>
{
    private readonly ISupplierWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public CreateSupplierHandler(ISupplierWriteStore writeStore, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result<Guid>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            request.CompanyName,
            PhoneNumber.Create(request.PhoneNumber),
            request.ContactPerson,
            Email.Create(request.Email),
            request.VatNumber,
            request.Street is not null && request.City is not null
                ? Address.Create(request.Street, request.City, request.PostalCode, request.Country)
                : null);

        await _writeStore.InsertAsync(supplier, cancellationToken);
        await _eventBus.PublishAsync(
            new SupplierCreatedIntegrationEvent(supplier.Id, supplier.SupplierCode, supplier.CompanyName, supplier.PhoneNumber.Value, supplier.Email?.Value),
            cancellationToken);
        supplier.ClearDomainEvents();
        return Result.Success(supplier.Id);
    }
}

public sealed record SupplierCreatedIntegrationEvent(
    Guid SupplierId,
    string SupplierCode,
    string CompanyName,
    string PhoneNumber,
    string? Email) : RMS.BuildingBlocks.EventBus.IntegrationEvent;
