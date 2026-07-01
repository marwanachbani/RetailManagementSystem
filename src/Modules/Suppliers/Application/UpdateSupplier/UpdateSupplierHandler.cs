using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.Modules.Suppliers.Domain.Entities;
using RMS.Modules.Suppliers.Domain.ValueObjects;

namespace RMS.Modules.Suppliers.Application.UpdateSupplier;

public sealed class UpdateSupplierHandler : IRequestHandler<UpdateSupplierCommand, Result>
{
    private readonly ISupplierReadStore _readStore;
    private readonly ISupplierWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdateSupplierHandler(ISupplierReadStore readStore, ISupplierWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var model = await _readStore.GetByIdAsync(request.SupplierId, cancellationToken);
        if (model is null)
            return Result.Failure("Supplier not found.", "Supplier.NotFound");

        var supplier = Supplier.Rehydrate(
            model.Id, model.SupplierCode, model.CompanyName, model.ContactPerson,
            PhoneNumber.Create(model.PhoneNumber), Email.Create(model.Email),
            model.VatNumber, model.Street is not null && model.City is not null
                ? Address.Create(model.Street, model.City, model.PostalCode, model.Country)
                : null,
            Enum.Parse<SupplierStatus>(model.Status), model.CreatedAt, model.UpdatedAt);

        supplier.Update(
            request.CompanyName,
            PhoneNumber.Create(request.PhoneNumber),
            request.ContactPerson,
            Email.Create(request.Email),
            request.VatNumber,
            request.Street is not null && request.City is not null
                ? Address.Create(request.Street, request.City, request.PostalCode, request.Country)
                : null);

        await _writeStore.UpdateAsync(supplier, cancellationToken);
        await _eventBus.PublishAsync(
            new SupplierUpdatedIntegrationEvent(supplier.Id, supplier.SupplierCode, supplier.CompanyName, supplier.PhoneNumber.Value),
            cancellationToken);
        supplier.ClearDomainEvents();
        return Result.Success();
    }
}

public sealed record SupplierUpdatedIntegrationEvent(Guid SupplierId, string SupplierCode, string CompanyName, string PhoneNumber) : RMS.BuildingBlocks.EventBus.IntegrationEvent;
