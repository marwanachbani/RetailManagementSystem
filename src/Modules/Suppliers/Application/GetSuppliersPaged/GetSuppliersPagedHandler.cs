using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.GetSuppliersPaged;

public sealed class GetSuppliersPagedHandler : IRequestHandler<GetSuppliersPagedQuery, Result<PagedResult<SupplierReadModel>>>
{
    private readonly ISupplierReadStore _readStore;

    public GetSuppliersPagedHandler(ISupplierReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<PagedResult<SupplierReadModel>>> Handle(GetSuppliersPagedQuery request, CancellationToken cancellationToken)
    {
        var page = await _readStore.GetPagedAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.IncludeInactive, cancellationToken);
        return Result.Success(page);
    }
}
