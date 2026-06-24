using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;

namespace RMS.Modules.Products.Application.SearchProducts;

public sealed class SearchProductsHandler : IRequestHandler<SearchProductsQuery, Result<IReadOnlyList<ProductReadModel>>>
{
    private readonly IProductReadStore _readStore;

    public SearchProductsHandler(IProductReadStore readStore) => _readStore = readStore;

    public async Task<Result<IReadOnlyList<ProductReadModel>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _readStore.SearchAsync(request.SearchTerm, request.IncludeInactive, cancellationToken);
        return Result.Success(products);
    }
}
