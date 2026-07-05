using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.GetSupplierById;

public sealed class GetSupplierByIdHandler : IRequestHandler<GetSupplierByIdQuery, Result<SupplierReadModel>>
{
    private readonly ISupplierReadStore _readStore;

    public GetSupplierByIdHandler(ISupplierReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<SupplierReadModel>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _readStore.GetByIdAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return Result.Failure<SupplierReadModel>("Supplier not found.", "Supplier.NotFound");

        return Result.Success(supplier);
    }
}
