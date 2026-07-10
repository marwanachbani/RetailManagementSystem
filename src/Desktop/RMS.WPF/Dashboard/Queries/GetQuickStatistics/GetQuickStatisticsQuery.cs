using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetQuickStatistics;

public sealed record GetQuickStatisticsQuery : IRequest<Result<QuickStatistics>>;
