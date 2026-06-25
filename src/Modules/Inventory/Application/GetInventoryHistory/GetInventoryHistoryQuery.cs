using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;

namespace RMS.Modules.Inventory.Application.GetInventoryHistory;

public sealed record GetInventoryHistoryQuery(Guid InventoryItemId) : IRequest<Result<IReadOnlyList<InventoryTransactionReadModel>>>;
