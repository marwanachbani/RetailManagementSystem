using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;

namespace RMS.Modules.Products.Application.GetProductsPaged;

public sealed class GetProductsPagedHandler : IRequestHandler<GetProductsPagedQuery, Result<PagedResult<ProductReadModel>>>
{
    private readonly IProductReadStore _readStore;

    public GetProductsPagedHandler(IProductReadStore readStore) => _readStore = readStore;

    public async Task<Result<PagedResult<ProductReadModel>>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var page = await _readStore.GetPagedAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.IncludeInactive, cancellationToken);
        return Result.Success(page);
    }
}
