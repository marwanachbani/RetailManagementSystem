using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;
using RMS.Modules.Sales.Domain.Entities;
using RMS.Modules.Sales.Application;

namespace RMS.Modules.Sales.Application.CreateSale;

public sealed class CreateSaleHandler : IRequestHandler<CreateSaleCommand, Result<Guid>>
{
    private readonly ISaleWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public CreateSaleHandler(ISaleWriteStore writeStore, IEventBus eventBus)
    {
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result<Guid>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = Sale.Create(Guid.NewGuid(), request.CashierId, request.Notes);

        await _writeStore.InsertAsync(sale, cancellationToken);
        await _eventBus.PublishAsync(new SaleCreatedIntegrationEvent(sale.Id, sale.SaleNumber, sale.CashierId), cancellationToken);
        sale.ClearDomainEvents();
        return Result.Success(sale.Id);
    }
}
