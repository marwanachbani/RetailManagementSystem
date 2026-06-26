using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;

namespace RMS.Modules.Sales.Application.GetSaleById;

public sealed class GetSaleByIdHandler : IRequestHandler<GetSaleByIdQuery, Result<SaleReadModel>>
{
    private readonly ISaleReadStore _readStore;

    public GetSaleByIdHandler(ISaleReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<SaleReadModel>> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _readStore.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
            return Result.Failure<SaleReadModel>("Sale not found.", "Sales.NotFound");

        return Result.Success(sale);
    }
}
