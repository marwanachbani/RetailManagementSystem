using RMS.Modules.Sales.Domain.Entities;

namespace RMS.Modules.Sales.Application.Contracts;

public sealed record SaleItemReadModel(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice)
{
    private SaleItemReadModel() : this(default, default, "", 0, 0, 0) { }
}

public sealed record SaleReadModel(
    Guid Id,
    string SaleNumber,
    Guid CashierId,
    DateTime SaleDate,
    string Status,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal DiscountPercentage,
    decimal TaxPercentage,
    DateTime? CompletedAt,
    DateTime? RefundedAt,
    DateTime CreatedAt,
    string? Notes,
    IReadOnlyList<SaleItemReadModel> Items)
{
    private SaleReadModel() : this(default, "", default, default, "", 0, 0, 0, 0, 0, 0, null, null, default, null, new List<SaleItemReadModel>()) { }
}

public sealed record ReceiptReadModel(
    Guid Id,
    Guid SaleId,
    string ReceiptNumber,
    string? PdfPath,
    DateTime GeneratedAt,
    string? StoreName,
    string? CashierName,
    decimal TotalAmount)
{
    private ReceiptReadModel() : this(default, default, "", null, default, null, null, 0) { }
}

public sealed record DailySalesSummary(
    DateTime Date,
    int TotalSales,
    decimal TotalRevenue,
    decimal TotalDiscounts,
    decimal TotalTaxes)
{
    private DailySalesSummary() : this(default, 0, 0, 0, 0) { }
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public interface ISaleReadStore
{
    Task<SaleReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SaleItemReadModel>> GetItemsBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
    Task<ReceiptReadModel?> GetReceiptBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
    Task<PagedResult<SaleReadModel>> GetPagedAsync(int pageNumber, int pageSize, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SaleReadModel>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<DailySalesSummary> GetDailySummaryAsync(DateTime date, CancellationToken cancellationToken = default);
}

public interface ISaleWriteStore
{
    Task InsertAsync(Sale sale, CancellationToken cancellationToken = default);
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);
    Task InsertReceiptAsync(Receipt receipt, CancellationToken cancellationToken = default);
}
