using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.Modules.Suppliers.Domain.Entities;
using RMS.Modules.Suppliers.Domain.ValueObjects;

namespace RMS.Modules.Suppliers.Application.DeactivateSupplier;

public sealed class DeactivateSupplierHandler : IRequestHandler<DeactivateSupplierCommand, Result>
{
    private readonly ISupplierReadStore _readStore;
    private readonly ISupplierWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public DeactivateSupplierHandler(ISupplierReadStore readStore, ISupplierWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeactivateSupplierCommand request, CancellationToken cancellationToken)
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

        supplier.Deactivate();

        await _writeStore.UpdateAsync(supplier, cancellationToken);
        await _eventBus.PublishAsync(
            new SupplierDeactivatedIntegrationEvent(supplier.Id, supplier.SupplierCode),
            cancellationToken);
        supplier.ClearDomainEvents();
        return Result.Success();
    }
}

public sealed record SupplierDeactivatedIntegrationEvent(Guid SupplierId, string SupplierCode) : RMS.BuildingBlocks.EventBus.IntegrationEvent;
