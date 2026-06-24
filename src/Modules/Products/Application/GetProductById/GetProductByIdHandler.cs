using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;

namespace RMS.Modules.Products.Application.GetProductById;

public sealed class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ProductReadModel>>
{
    private readonly IProductReadStore _readStore;

    public GetProductByIdHandler(IProductReadStore readStore) => _readStore = readStore;

    public async Task<Result<ProductReadModel>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _readStore.GetByIdAsync(request.Id, cancellationToken);
        return product is null
            ? Result.Failure<ProductReadModel>("Product was not found.", "Products.NotFound")
            : Result.Success(product);
    }
}
