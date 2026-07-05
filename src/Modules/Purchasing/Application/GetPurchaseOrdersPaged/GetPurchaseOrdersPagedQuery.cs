using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;

namespace RMS.Modules.Purchasing.Application.GetPurchaseOrdersPaged;

public sealed record GetPurchaseOrdersPagedQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    int? StatusFilter) : IRequest<Result<PagedResult<PurchaseOrderReadModel>>>;
