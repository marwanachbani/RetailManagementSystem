using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Sales.Application.CreateSale;

public sealed record CreateSaleCommand(
    Guid CashierId,
    string? Notes = null) : IRequest<Result<Guid>>;
