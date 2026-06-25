using RMS.Modules.Inventory.Domain.Entities;

namespace RMS.Modules.Inventory.Application.Contracts;

public sealed record InventoryItemReadModel(
    Guid Id,
    Guid ProductId,
    int CurrentQuantity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int LowStockThreshold)
{
    private InventoryItemReadModel() : this(default, default, 0, false, default, null, 0) { }
}

public sealed record InventoryTransactionReadModel(
    Guid Id,
    Guid InventoryItemId,
    Guid ProductId,
    int QuantityBefore,
    int QuantityAfter,
    int ChangeAmount,
    string Reason,
    Guid? UserId,
    DateTime Timestamp)
{
    private InventoryTransactionReadModel() : this(default, default, default, 0, 0, 0, "", null, default) { }
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public interface IInventoryReadStore
{
    Task<InventoryItemReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryItemReadModel?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItemReadModel>> GetLowStockItemsAsync(int threshold, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryTransactionReadModel>> GetHistoryAsync(Guid inventoryItemId, CancellationToken cancellationToken = default);
    Task<PagedResult<InventoryItemReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default);
}

public interface IInventoryWriteStore
{
    Task InsertAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);
    Task InsertTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default);
}
