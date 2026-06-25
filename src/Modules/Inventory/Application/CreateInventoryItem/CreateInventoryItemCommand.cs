using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Inventory.Application.CreateInventoryItem;

public sealed record CreateInventoryItemCommand(
    Guid ProductId,
    int InitialQuantity = 0,
    int LowStockThreshold = 10) : IRequest<Result<Guid>>;
