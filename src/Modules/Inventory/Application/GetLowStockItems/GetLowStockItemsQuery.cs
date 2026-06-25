using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;

namespace RMS.Modules.Inventory.Application.GetLowStockItems;

public sealed record GetLowStockItemsQuery(int Threshold) : IRequest<Result<IReadOnlyList<InventoryItemReadModel>>>;
