using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Sales.Application.CompleteSale;

public sealed record CompleteSaleCommand(
    Guid SaleId,
    decimal DiscountPercentage = 0,
    decimal TaxPercentage = 0) : IRequest<Result>;
