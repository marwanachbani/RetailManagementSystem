using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Sales.Application.RemoveSaleItem;

public sealed record RemoveSaleItemCommand(
    Guid SaleId,
    Guid SaleItemId) : IRequest<Result>;
