using RMS.Modules.Purchasing.Domain.Entities;

namespace RMS.Modules.Purchasing.Application.Contracts;

public sealed record PurchaseOrderItemReadModel(
    Guid Id,
    Guid PurchaseOrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitCost,
    decimal TotalCost,
    int ReceivedQuantity)
{
    private PurchaseOrderItemReadModel() : this(default, default, default, "", 0, 0, 0, 0) { }
    public int Remaining => Quantity - ReceivedQuantity;
}

public sealed record GoodsReceiptReadModel(
    Guid Id,
    Guid PurchaseOrderId,
    Guid ProductId,
    int QuantityReceived,
    DateTime ReceivedAt,
    string? BatchNumber,
    DateTime? ExpiryDate)
{
    private GoodsReceiptReadModel() : this(default, default, default, 0, default, null, null) { }
}

public sealed record PurchaseOrderReadModel(
    Guid Id,
    string PurchaseNumber,
    Guid SupplierId,
    string SupplierName,
    DateTime OrderDate,
    string Status,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal TaxPercentage,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    DateTime CreatedAt,
    string? Notes,
    string? SupplierInvoiceNumber,
    IReadOnlyList<PurchaseOrderItemReadModel> Items,
    IReadOnlyList<GoodsReceiptReadModel> GoodsReceipts)
{
    private PurchaseOrderReadModel() : this(default, "", default, "", default, "", 0, 0, 0, 0, null, null, default, null, null, new List<PurchaseOrderItemReadModel>(), new List<GoodsReceiptReadModel>()) { }
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public interface IPurchaseOrderReadStore
{
    Task<PurchaseOrderReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseOrderItemReadModel>> GetItemsByPurchaseOrderIdAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GoodsReceiptReadModel>> GetGoodsReceiptsByPurchaseOrderIdAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default);
    Task<PagedResult<PurchaseOrderReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, int? statusFilter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseOrderReadModel>> SearchAsync(string? searchTerm, int? statusFilter, CancellationToken cancellationToken = default);
}

public interface IPurchaseOrderWriteStore
{
    Task InsertAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
    Task UpdateAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
    Task InsertGoodsReceiptAsync(GoodsReceipt goodsReceipt, CancellationToken cancellationToken = default);
}
