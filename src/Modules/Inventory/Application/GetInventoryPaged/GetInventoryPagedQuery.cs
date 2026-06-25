using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.Contracts;

namespace RMS.Modules.Inventory.Application.GetInventoryPaged;

public sealed record GetInventoryPagedQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    bool IncludeInactive) : IRequest<Result<PagedResult<InventoryItemReadModel>>>;
