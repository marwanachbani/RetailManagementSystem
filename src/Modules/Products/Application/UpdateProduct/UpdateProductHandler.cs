using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Domain.Entities;
using RMS.Modules.Products.Domain.ValueObjects;

namespace RMS.Modules.Products.Application.UpdateProduct;

public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IProductReadStore _readStore;
    private readonly IProductWriteStore _writeStore;

    public UpdateProductHandler(IProductReadStore readStore, IProductWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var current = await _readStore.GetByIdAsync(request.Id, cancellationToken);
        if (current is null)
            return Result.Failure("Product was not found.", "Products.NotFound");

        var duplicateBarcode = await _readStore.GetByBarcodeAsync(request.Barcode, cancellationToken);
        if (duplicateBarcode is not null && duplicateBarcode.Id != request.Id)
            return Result.Failure("A product with this barcode already exists.", "Products.BarcodeAlreadyExists");

        var product = Product.Create(
            current.Id,
            current.Name,
            current.Description,
            Barcode.Create(current.Barcode),
            current.CategoryId,
            Money.Create(current.SalePrice),
            Money.Create(current.CostPrice));
        product.ClearDomainEvents();

        product.Update(
            request.Name,
            request.Description,
            Barcode.Create(request.Barcode),
            request.CategoryId,
            Money.Create(request.SalePrice),
            Money.Create(request.CostPrice));

        await _writeStore.UpdateAsync(product, cancellationToken);
        product.ClearDomainEvents();
        return Result.Success();
    }
}
