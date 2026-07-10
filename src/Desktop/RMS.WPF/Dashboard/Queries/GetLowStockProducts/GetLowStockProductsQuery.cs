using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.WPF.Dashboard.Queries.GetLowStockProducts;

public sealed record GetLowStockProductsQuery(int Limit = 10) : IRequest<Result<IReadOnlyList<LowStockProductDto>>>;
