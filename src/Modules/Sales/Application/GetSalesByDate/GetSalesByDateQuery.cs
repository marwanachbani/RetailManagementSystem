using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;

namespace RMS.Modules.Sales.Application.GetSalesByDate;

public sealed record GetSalesByDateQuery(DateTime Date) : IRequest<Result<IReadOnlyList<SaleReadModel>>>;
