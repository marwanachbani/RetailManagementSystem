using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Products.Application.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    string Barcode,
    Guid CategoryId,
    decimal SalePrice,
    decimal CostPrice) : IRequest<Result>;
