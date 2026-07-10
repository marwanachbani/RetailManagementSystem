using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery : IRequest<Result<KpiSummary>>;
