namespace RMS.WPF.Dashboard;

public sealed record KpiSummary(
    int TodaysSales,
    decimal TodaysRevenue,
    decimal MonthlyRevenue,
    int TotalProducts,
    int ActiveCustomers,
    int ActiveSuppliers,
    int PurchaseOrdersToday,
    int LowStockProducts,
    int OutOfStockProducts,
    decimal InventoryValue)
{
    public KpiSummary() : this(0, 0, 0, 0, 0, 0, 0, 0, 0, 0) { }
}

public sealed record RecentSaleDto(
    Guid Id,
    string SaleNumber,
    decimal TotalAmount,
    DateTime SaleDate,
    string Status)
{
    private RecentSaleDto() : this(default, "", 0, default, "") { }
}

public sealed record RecentPurchaseDto(
    Guid Id,
    string PurchaseNumber,
    string SupplierName,
    decimal TotalAmount,
    DateTime OrderDate,
    string Status)
{
    private RecentPurchaseDto() : this(default, "", "", 0, default, "") { }
}

public sealed record LowStockProductDto(
    Guid ProductId,
    string ProductName,
    int CurrentQuantity,
    int LowStockThreshold)
{
    private LowStockProductDto() : this(default, "", 0, 0) { }
}

public sealed record ActivityDto(
    string ActivityType,
    string Description,
    DateTime Timestamp,
    string IconGlyph)
{
    private ActivityDto() : this("", "", default, "") { }
}

public sealed record QuickStatistics(
    int SalesThisWeek,
    int SalesThisMonth,
    decimal RevenueThisWeek,
    decimal RevenueThisMonth,
    int NewCustomersThisMonth,
    int NewProductsThisMonth)
{
    public QuickStatistics() : this(0, 0, 0, 0, 0, 0) { }
}

public sealed record AlertDto(
    string Title,
    string Message,
    string Severity,
    int Count)
{
    private AlertDto() : this("", "", "", 0) { }
}
