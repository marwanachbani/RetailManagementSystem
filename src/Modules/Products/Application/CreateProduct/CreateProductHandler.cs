using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Domain.Entities;
using RMS.Modules.Products.Domain.ValueObjects;

namespace RMS.Modules.Products.Application.CreateProduct;

public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductReadStore _readStore;
    private readonly IProductWriteStore _writeStore;

    public CreateProductHandler(IProductReadStore readStore, IProductWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await _readStore.GetByBarcodeAsync(request.Barcode, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>("A product with this barcode already exists.", "Products.BarcodeAlreadyExists");

        var product = Product.Create(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            Barcode.Create(request.Barcode),
            request.CategoryId,
            Money.Create(request.SalePrice),
            Money.Create(request.CostPrice));

        await _writeStore.InsertAsync(product, cancellationToken);
        product.ClearDomainEvents();
        return Result.Success(product.Id);
    }
}
