using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetRecentPurchases;

public sealed record GetRecentPurchasesQuery(int Limit = 5) : IRequest<Result<IReadOnlyList<RecentPurchaseDto>>>;
