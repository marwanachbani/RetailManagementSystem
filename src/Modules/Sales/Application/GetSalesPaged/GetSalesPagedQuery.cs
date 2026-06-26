using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;

namespace RMS.Modules.Sales.Application.GetSalesPaged;

public sealed record GetSalesPagedQuery(
    int PageNumber,
    int PageSize,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<Result<PagedResult<SaleReadModel>>>;
