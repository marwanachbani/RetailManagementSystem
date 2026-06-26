using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.Contracts;

namespace RMS.Modules.Sales.Application.GetSaleById;

public sealed record GetSaleByIdQuery(Guid SaleId) : IRequest<Result<SaleReadModel>>;
