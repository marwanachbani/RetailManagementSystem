using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Inventory.Application.DecreaseStock;

public sealed record DecreaseStockCommand(
    Guid InventoryItemId,
    int Amount,
    string Reason,
    Guid? UserId = null) : IRequest<Result>;
