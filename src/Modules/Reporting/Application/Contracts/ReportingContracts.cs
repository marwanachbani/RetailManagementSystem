using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Reporting.Application.Contracts;

public sealed record DateRangeFilter(
    DateTime? FromDate,
    DateTime? ToDate)
{
    public DateRangeFilter() : this(null, null) { }
}

public sealed record ReportFilter(
    string? SearchTerm,
    DateRangeFilter? DateRange,
    string? SortColumn,
    bool SortDescending)
{
    public ReportFilter() : this(null, null, null, false) { }
}

public sealed record SalesReportItem(
    Guid Id,
    string SaleNumber,
    DateTime SaleDate,
    string Status,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string? CustomerName,
    string? CashierName)
{
    private SalesReportItem() : this(default, "", default, "", 0, 0, 0, 0, null, null) { }
}

public sealed record SalesReportResult(
    IReadOnlyList<SalesReportItem> Items,
    int TotalCount,
    decimal GrandTotalRevenue,
    decimal GrandTotalDiscount,
    decimal GrandTotalTax,
    decimal GrandTotalNet)
{
    public SalesReportResult() : this(new List<SalesReportItem>(), 0, 0, 0, 0, 0) { }
}

public sealed record InventoryReportItem(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string CategoryName,
    int CurrentQuantity,
    int LowStockThreshold,
    decimal CostPrice,
    decimal SalePrice,
    decimal TotalValue)
{
    private InventoryReportItem() : this(default, "", "", "", 0, 0, 0, 0, 0) { }
}

public sealed record InventoryReportResult(
    IReadOnlyList<InventoryReportItem> Items,
    int TotalCount,
    int TotalProducts,
    int LowStockCount,
    int OutOfStockCount,
    decimal TotalInventoryValue)
{
    public InventoryReportResult() : this(new List<InventoryReportItem>(), 0, 0, 0, 0, 0) { }
}

public sealed record StockMovementItem(
    Guid TransactionId,
    Guid ProductId,
    string ProductName,
    int ChangeAmount,
    int QuantityBefore,
    int QuantityAfter,
    string Reason,
    DateTime Timestamp)
{
    private StockMovementItem() : this(default, default, "", 0, 0, 0, "", default) { }
}

public sealed record StockMovementResult(
    IReadOnlyList<StockMovementItem> Items,
    int TotalCount)
{
    public StockMovementResult() : this(new List<StockMovementItem>(), 0) { }
}

public sealed record PurchaseReportItem(
    Guid PurchaseOrderId,
    string PurchaseNumber,
    Guid SupplierId,
    string SupplierName,
    DateTime OrderDate,
    string Status,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    int ItemsCount)
{
    private PurchaseReportItem() : this(default, "", default, "", default, "", 0, 0, 0, 0) { }
}

public sealed record PurchaseReportResult(
    IReadOnlyList<PurchaseReportItem> Items,
    int TotalCount,
    decimal GrandTotalCost)
{
    public PurchaseReportResult() : this(new List<PurchaseReportItem>(), 0, 0) { }
}

public sealed record PurchaseByProductItem(
    Guid ProductId,
    string ProductName,
    int TotalQuantity,
    decimal TotalCost,
    int OrderCount)
{
    private PurchaseByProductItem() : this(default, "", 0, 0, 0) { }
}

public sealed record PurchaseByProductResult(
    IReadOnlyList<PurchaseByProductItem> Items,
    int TotalCount,
    decimal GrandTotalCost)
{
    public PurchaseByProductResult() : this(new List<PurchaseByProductItem>(), 0, 0) { }
}

public sealed record CustomerReportItem(
    Guid CustomerId,
    string CustomerCode,
    string FullName,
    string? PhoneNumber,
    string? Email,
    int TotalSales,
    decimal TotalSpent,
    decimal AverageOrderValue,
    DateTime? FirstPurchaseDate,
    DateTime? LastPurchaseDate)
{
    private CustomerReportItem() : this(default, "", "", null, null, 0, 0, 0, null, null) { }
}

public sealed record CustomerReportResult(
    IReadOnlyList<CustomerReportItem> Items,
    int TotalCount,
    int ActiveCustomers,
    int InactiveCustomers)
{
    public CustomerReportResult() : this(new List<CustomerReportItem>(), 0, 0, 0) { }
}

public sealed record SupplierReportItem(
    Guid SupplierId,
    string SupplierCode,
    string CompanyName,
    string? ContactPerson,
    string? PhoneNumber,
    int TotalOrders,
    decimal TotalPurchases,
    decimal AverageOrderValue,
    DateTime? LastOrderDate)
{
    private SupplierReportItem() : this(default, "", "", null, null, 0, 0, 0, null) { }
}

public sealed record SupplierReportResult(
    IReadOnlyList<SupplierReportItem> Items,
    int TotalCount,
    int ActiveSuppliers,
    int InactiveSuppliers)
{
    public SupplierReportResult() : this(new List<SupplierReportItem>(), 0, 0, 0) { }
}

public sealed record ProductReportItem(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string CategoryName,
    decimal SalePrice,
    decimal CostPrice,
    int TotalSold,
    decimal TotalRevenue,
    decimal TotalProfit,
    int StockLevel)
{
    private ProductReportItem() : this(default, "", "", "", 0, 0, 0, 0, 0, 0) { }
}

public sealed record ProductReportResult(
    IReadOnlyList<ProductReportItem> Items,
    int TotalCount,
    int TotalProducts)
{
    public ProductReportResult() : this(new List<ProductReportItem>(), 0, 0) { }
}

public sealed record FinancialReportItem(
    string Period,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Revenue,
    decimal CostOfGoodsSold,
    decimal GrossProfit,
    decimal SalesTax,
    decimal NetProfit)
{
    private FinancialReportItem() : this("", default, default, 0, 0, 0, 0, 0) { }
}

public sealed record FinancialReportResult(
    IReadOnlyList<FinancialReportItem> Items,
    int TotalPeriods,
    decimal TotalRevenue,
    decimal TotalCostOfGoodsSold,
    decimal TotalGrossProfit,
    decimal TotalSalesTax,
    decimal TotalNetProfit)
{
    public FinancialReportResult() : this(new List<FinancialReportItem>(), 0, 0, 0, 0, 0, 0) { }
}

public interface IReportingReadStore
{
    Task<SalesReportResult> GetSalesReportAsync(DateRangeFilter dateRange, string? searchTerm, string? sortColumn, bool sortDescending, CancellationToken cancellationToken = default);
    Task<InventoryReportResult> GetInventoryReportAsync(string? searchTerm, CancellationToken cancellationToken = default);
    Task<StockMovementResult> GetStockMovementAsync(DateRangeFilter dateRange, string? searchTerm, CancellationToken cancellationToken = default);
    Task<PurchaseReportResult> GetPurchaseReportAsync(DateRangeFilter dateRange, Guid? supplierId, string? searchTerm, string? sortColumn, bool sortDescending, CancellationToken cancellationToken = default);
    Task<PurchaseByProductResult> GetPurchaseByProductAsync(DateRangeFilter dateRange, string? searchTerm, CancellationToken cancellationToken = default);
    Task<CustomerReportResult> GetCustomerReportAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default);
    Task<SupplierReportResult> GetSupplierReportAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default);
    Task<ProductReportResult> GetProductReportAsync(string? searchTerm, string? sortColumn, bool sortDescending, CancellationToken cancellationToken = default);
    Task<FinancialReportResult> GetFinancialReportAsync(DateRangeFilter dateRange, string periodType, CancellationToken cancellationToken = default);
}
