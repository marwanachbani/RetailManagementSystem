using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Purchasing.Application.Contracts;

namespace RMS.Modules.Purchasing.Application.SearchPurchaseOrders;

public sealed record SearchPurchaseOrdersQuery(
    string? SearchTerm,
    int? StatusFilter) : IRequest<Result<IReadOnlyList<PurchaseOrderReadModel>>>;
