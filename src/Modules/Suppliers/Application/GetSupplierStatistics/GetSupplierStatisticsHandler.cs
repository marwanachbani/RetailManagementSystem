using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Suppliers.Application.Contracts;

namespace RMS.Modules.Suppliers.Application.GetSupplierStatistics;

public sealed class GetSupplierStatisticsHandler : IRequestHandler<GetSupplierStatisticsQuery, Result<SupplierStatisticsReadModel>>
{
    private readonly ISupplierReadStore _readStore;

    public GetSupplierStatisticsHandler(ISupplierReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<SupplierStatisticsReadModel>> Handle(GetSupplierStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _readStore.GetStatisticsAsync(request.SupplierId, cancellationToken);
        if (stats is null)
            return Result.Failure<SupplierStatisticsReadModel>("Supplier not found.", "Supplier.NotFound");

        return Result.Success(stats);
    }
}
