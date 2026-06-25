using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Inventory.Application.AdjustStock;

public sealed record AdjustStockCommand(
    Guid InventoryItemId,
    int NewQuantity,
    string Reason,
    Guid? UserId = null) : IRequest<Result>;
