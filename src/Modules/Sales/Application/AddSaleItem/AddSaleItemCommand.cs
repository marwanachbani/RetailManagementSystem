using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Sales.Application.AddSaleItem;

public sealed record AddSaleItemCommand(
    Guid SaleId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice) : IRequest<Result>;
