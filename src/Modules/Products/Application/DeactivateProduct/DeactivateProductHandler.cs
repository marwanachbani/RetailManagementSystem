using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Domain.Entities;
using RMS.Modules.Products.Domain.ValueObjects;

namespace RMS.Modules.Products.Application.DeactivateProduct;

public sealed class DeactivateProductHandler : IRequestHandler<DeactivateProductCommand, Result>
{
    private readonly IProductReadStore _readStore;
    private readonly IProductWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public DeactivateProductHandler(IProductReadStore readStore, IProductWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        var current = await _readStore.GetByIdAsync(request.Id, cancellationToken);
        if (current is null)
            return Result.Failure("Product was not found.", "Products.NotFound");

        var product = Product.Rehydrate(
            current.Id,
            current.ProductCode,
            current.Name,
            current.Description,
            Barcode.Create(current.Barcode),
            current.CategoryId,
            Money.Create(current.SalePrice),
            Money.Create(current.CostPrice),
            current.IsActive,
            current.CreatedAt,
            current.UpdatedAt);

        product.Deactivate();
        await _writeStore.UpdateAsync(product, cancellationToken);
        await _eventBus.PublishAsync(new ProductDeactivatedIntegrationEvent(product.Id, product.ProductCode), cancellationToken);
        product.ClearDomainEvents();
        return Result.Success();
    }
}
