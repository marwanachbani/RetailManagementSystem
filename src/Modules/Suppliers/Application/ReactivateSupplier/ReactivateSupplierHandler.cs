using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.Modules.Suppliers.Domain.Entities;
using RMS.Modules.Suppliers.Domain.ValueObjects;

namespace RMS.Modules.Suppliers.Application.ReactivateSupplier;

public sealed class ReactivateSupplierHandler : IRequestHandler<ReactivateSupplierCommand, Result>
{
    private readonly ISupplierReadStore _readStore;
    private readonly ISupplierWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public ReactivateSupplierHandler(ISupplierReadStore readStore, ISupplierWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ReactivateSupplierCommand request, CancellationToken cancellationToken)
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

        supplier.Reactivate();

        await _writeStore.UpdateAsync(supplier, cancellationToken);
        await _eventBus.PublishAsync(
            new SupplierReactivatedIntegrationEvent(supplier.Id, supplier.SupplierCode),
            cancellationToken);
        supplier.ClearDomainEvents();
        return Result.Success();
    }
}

public sealed record SupplierReactivatedIntegrationEvent(Guid SupplierId, string SupplierCode) : RMS.BuildingBlocks.EventBus.IntegrationEvent;
