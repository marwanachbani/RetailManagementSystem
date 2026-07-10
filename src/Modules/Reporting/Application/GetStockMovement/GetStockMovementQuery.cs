using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Application.GetStockMovement;

public sealed record GetStockMovementQuery(DateRangeFilter? DateRange, string? SearchTerm) : IRequest<Result<StockMovementResult>>;
