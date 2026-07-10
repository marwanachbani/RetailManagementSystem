using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetRecentActivities;

public sealed record GetRecentActivitiesQuery(int Limit = 10) : IRequest<Result<IReadOnlyList<ActivityDto>>>;
