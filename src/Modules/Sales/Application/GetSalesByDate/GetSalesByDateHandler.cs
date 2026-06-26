using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;

namespace RMS.Modules.Sales.Application.GetSalesByDate;

public sealed class GetSalesByDateHandler : IRequestHandler<GetSalesByDateQuery, Result<IReadOnlyList<SaleReadModel>>>
{
    private readonly ISaleReadStore _readStore;

    public GetSalesByDateHandler(ISaleReadStore readStore)
    {
        _readStore = readStore;
    }

    public async Task<Result<IReadOnlyList<SaleReadModel>>> Handle(GetSalesByDateQuery request, CancellationToken cancellationToken)
    {
        var result = await _readStore.GetByDateAsync(request.Date, cancellationToken);
        return Result.Success(result);
    }
}
