using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Products.Application.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    string Barcode,
    Guid CategoryId,
    decimal SalePrice,
    decimal CostPrice) : IRequest<Result<Guid>>;
