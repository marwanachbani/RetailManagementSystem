using MediatR;
using RMS.BuildingBlocks.EventBus;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Domain.Entities;
using RMS.Modules.Products.Domain.ValueObjects;

namespace RMS.Modules.Products.Application.UpdateProduct;

public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IProductReadStore _readStore;
    private readonly IProductWriteStore _writeStore;
    private readonly IEventBus _eventBus;

    public UpdateProductHandler(IProductReadStore readStore, IProductWriteStore writeStore, IEventBus eventBus)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var current = await _readStore.GetByIdAsync(request.Id, cancellationToken);
        if (current is null)
            return Result.Failure("Product was not found.", "Products.NotFound");

        var duplicateBarcode = await _readStore.GetByBarcodeAsync(request.Barcode, cancellationToken);
        if (duplicateBarcode is not null && duplicateBarcode.Id != request.Id)
            return Result.Failure("A product with this barcode already exists.", "Products.BarcodeAlreadyExists");

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

        product.Update(
            request.Name,
            request.Description,
            Barcode.Create(request.Barcode),
            request.CategoryId,
            Money.Create(request.SalePrice),
            Money.Create(request.CostPrice));

        await _writeStore.UpdateAsync(product, cancellationToken);
        await _eventBus.PublishAsync(new ProductUpdatedIntegrationEvent(product.Id, product.ProductCode, product.Name), cancellationToken);
        product.ClearDomainEvents();
        return Result.Success();
    }
}
