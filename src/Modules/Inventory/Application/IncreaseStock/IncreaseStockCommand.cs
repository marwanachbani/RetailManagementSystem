using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Inventory.Application.IncreaseStock;

public sealed record IncreaseStockCommand(
    Guid InventoryItemId,
    int Amount,
    string Reason,
    Guid? UserId = null) : IRequest<Result>;
