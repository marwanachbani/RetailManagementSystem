using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;

namespace RMS.Modules.Sales.Application.GetDailySalesSummary;

public sealed record GetDailySalesSummaryQuery(DateTime Date) : IRequest<Result<DailySalesSummary>>;
