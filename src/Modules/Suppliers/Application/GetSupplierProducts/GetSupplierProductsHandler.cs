using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.GetSupplierProducts;

public sealed class GetSupplierProductsHandler : IRequestHandler<GetSupplierProductsQuery, Result<IReadOnlyList<SupplierProductReadModel>>>
{
    private readonly ISupplierReadStore _readStore;

    public GetSupplierProductsHandler(ISupplierReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<IReadOnlyList<SupplierProductReadModel>>> Handle(GetSupplierProductsQuery request, CancellationToken cancellationToken)
    {
        var results = await _readStore.GetSupplierProductsAsync(request.SupplierId, cancellationToken);
        return Result.Success(results);
    }
}
