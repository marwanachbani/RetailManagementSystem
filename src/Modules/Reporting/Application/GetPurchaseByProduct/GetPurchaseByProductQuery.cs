using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetPurchaseByProduct;

public sealed record GetPurchaseByProductQuery(DateRangeFilter? DateRange, string? SearchTerm) : IRequest<Result<PurchaseByProductResult>>;
