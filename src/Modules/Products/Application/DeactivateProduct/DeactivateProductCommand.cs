using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Products.Application.DeactivateProduct;

public sealed record DeactivateProductCommand(Guid Id) : IRequest<Result>;
