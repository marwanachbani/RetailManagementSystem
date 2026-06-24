using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;

namespace RMS.Modules.Products.Application.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductReadModel>>;
