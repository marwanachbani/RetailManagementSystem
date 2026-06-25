using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;

namespace RMS.Modules.Inventory.Application.GetInventoryItem;

public sealed record GetInventoryItemQuery(Guid Id) : IRequest<Result<InventoryItemReadModel>>;
