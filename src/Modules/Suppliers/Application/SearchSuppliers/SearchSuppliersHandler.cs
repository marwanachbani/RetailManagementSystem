using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.SearchSuppliers;

public sealed class SearchSuppliersHandler : IRequestHandler<SearchSuppliersQuery, Result<IReadOnlyList<SupplierReadModel>>>
{
    private readonly ISupplierReadStore _readStore;

    public SearchSuppliersHandler(ISupplierReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<IReadOnlyList<SupplierReadModel>>> Handle(SearchSuppliersQuery request, CancellationToken cancellationToken)
    {
        var results = await _readStore.SearchAsync(request.SearchTerm, request.IncludeInactive, cancellationToken);
        return Result.Success(results);
    }
}
