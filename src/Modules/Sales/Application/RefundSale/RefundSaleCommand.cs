using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Sales.Application.RefundSale;

public sealed record RefundSaleCommand(
    Guid SaleId,
    string? Reason = null) : IRequest<Result>;
