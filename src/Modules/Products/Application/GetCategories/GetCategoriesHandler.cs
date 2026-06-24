using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;

namespace RMS.Modules.Products.Application.GetCategories;

public sealed class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryReadModel>>>
{
    private readonly IProductReadStore _readStore;

    public GetCategoriesHandler(IProductReadStore readStore) => _readStore = readStore;

    public async Task<Result<IReadOnlyList<CategoryReadModel>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _readStore.GetCategoriesAsync(cancellationToken);
        return Result.Success(categories);
    }
}
