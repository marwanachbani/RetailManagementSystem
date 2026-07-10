using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetRecentSales;

public sealed record GetRecentSalesQuery(int Limit = 5) : IRequest<Result<IReadOnlyList<RecentSaleDto>>>;
