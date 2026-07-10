using Dapper;
using Microsoft.Extensions.Logging;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.Modules.Reporting.Infrastructure.Persistence;

public sealed class ReportingReadStore : IReportingReadStore
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ReportingReadStore> _logger;

    public ReportingReadStore(IDbConnectionFactory connectionFactory, ILogger<ReportingReadStore> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<SalesReportResult> GetSalesReportAsync(DateRangeFilter dateRange, string? searchTerm, string? sortColumn, bool sortDescending, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (dateRange is { FromDate: not null })
        {
            whereClauses.Add("s.SaleDate >= @FromDate");
            parameters.Add("FromDate", dateRange.FromDate.Value);
        }
        if (dateRange is { ToDate: not null })
        {
            whereClauses.Add("s.SaleDate < @ToDate");
            parameters.Add("ToDate", dateRange.ToDate.Value.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("s.SaleNumber LIKE @SearchTerm");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        var orderBy = sortColumn switch
        {
            nameof(SalesReportItem.SaleDate) => "s.SaleDate",
            nameof(SalesReportItem.TotalAmount) => "s.TotalAmount",
            nameof(SalesReportItem.Status) => "s.Status",
            nameof(SalesReportItem.SaleNumber) => "s.SaleNumber",
            _ => "s.SaleDate"
        };
        var orderDir = sortDescending ? "DESC" : "ASC";

        var sql = $"""
            SELECT 
                s.Id,
                s.SaleNumber,
                s.SaleDate,
                CASE s.Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Refunded' END AS Status,
                s.SubTotal,
                s.DiscountAmount,
                s.TaxAmount,
                s.TotalAmount,
                c.FirstName || ' ' || c.LastName AS CustomerName,
                'Cashier' AS CashierName
            FROM Sales s
            LEFT JOIN Customers c ON s.CustomerId = c.Id
            {whereSql}
            ORDER BY {orderBy} {orderDir};
            """;

        var items = await connection.QueryAsync<SalesReportItem>(sql, parameters);
        var list = items.ToList();

        var result = new SalesReportResult(
            list,
            list.Count,
            list.Sum(x => x.SubTotal),
            list.Sum(x => x.DiscountAmount),
            list.Sum(x => x.TaxAmount),
            list.Sum(x => x.TotalAmount));

        return result;
    }

    public async Task<InventoryReportResult> GetInventoryReportAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClauses = new List<string> { "i.IsActive = 1" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("(p.Name LIKE @SearchTerm OR p.ProductCode LIKE @SearchTerm OR c.Name LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        var whereSql = "WHERE " + string.Join(" AND ", whereClauses);

        var sql = $"""
            SELECT 
                p.Id AS ProductId,
                p.ProductCode,
                p.Name AS ProductName,
                c.Name AS CategoryName,
                i.CurrentQuantity,
                i.LowStockThreshold,
                p.CostPrice,
                p.SalePrice,
                (p.CostPrice * i.CurrentQuantity) AS TotalValue
            FROM InventoryItems i
            JOIN Products p ON i.ProductId = p.Id
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            {whereSql}
            ORDER BY p.Name ASC;
            """;

        var items = await connection.QueryAsync<InventoryReportItem>(sql, parameters);
        var list = items.ToList();

        return new InventoryReportResult(
            list,
            list.Count,
            list.Count,
            list.Count(x => x.CurrentQuantity > 0 && x.CurrentQuantity <= x.LowStockThreshold),
            list.Count(x => x.CurrentQuantity == 0),
            list.Sum(x => x.TotalValue));
    }

    public async Task<StockMovementResult> GetStockMovementAsync(DateRangeFilter dateRange, string? searchTerm, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (dateRange is { FromDate: not null })
        {
            whereClauses.Add("t.Timestamp >= @FromDate");
            parameters.Add("FromDate", dateRange.FromDate.Value);
        }
        if (dateRange is { ToDate: not null })
        {
            whereClauses.Add("t.Timestamp < @ToDate");
            parameters.Add("ToDate", dateRange.ToDate.Value.AddDays(1));
        }
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("(p.Name LIKE @SearchTerm OR p.ProductCode LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        var sql = $"""
            SELECT 
                t.Id AS TransactionId,
                t.ProductId,
                p.Name AS ProductName,
                t.ChangeAmount,
                t.QuantityBefore,
                t.QuantityAfter,
                t.Reason,
                t.Timestamp
            FROM InventoryTransactions t
            JOIN Products p ON t.ProductId = p.Id
            {whereSql}
            ORDER BY t.Timestamp DESC;
            """;

        var items = await connection.QueryAsync<StockMovementItem>(sql, parameters);
        var list = items.ToList();

        return new StockMovementResult(list, list.Count);
    }

    public async Task<PurchaseReportResult> GetPurchaseReportAsync(DateRangeFilter dateRange, Guid? supplierId, string? searchTerm, string? sortColumn, bool sortDescending, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (dateRange is { FromDate: not null })
        {
            whereClauses.Add("po.OrderDate >= @FromDate");
            parameters.Add("FromDate", dateRange.FromDate.Value);
        }
        if (dateRange is { ToDate: not null })
        {
            whereClauses.Add("po.OrderDate < @ToDate");
            parameters.Add("ToDate", dateRange.ToDate.Value.AddDays(1));
        }
        if (supplierId.HasValue)
        {
            whereClauses.Add("po.SupplierId = @SupplierId");
            parameters.Add("SupplierId", supplierId.Value);
        }
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("(po.PurchaseNumber LIKE @SearchTerm OR po.SupplierName LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        var orderBy = sortColumn switch
        {
            nameof(PurchaseReportItem.OrderDate) => "po.OrderDate",
            nameof(PurchaseReportItem.TotalAmount) => "po.TotalAmount",
            nameof(PurchaseReportItem.SupplierName) => "po.SupplierName",
            nameof(PurchaseReportItem.Status) => "po.Status",
            _ => "po.OrderDate"
        };
        var orderDir = sortDescending ? "DESC" : "ASC";

        var sql = $"""
            SELECT 
                po.Id AS PurchaseOrderId,
                po.PurchaseNumber,
                po.SupplierId,
                po.SupplierName,
                po.OrderDate,
                CASE po.Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Cancelled' END AS Status,
                po.SubTotal,
                po.TaxAmount,
                po.TotalAmount,
                (SELECT COUNT(1) FROM PurchaseOrderItems WHERE PurchaseOrderId = po.Id) AS ItemsCount
            FROM PurchaseOrders po
            {whereSql}
            ORDER BY {orderBy} {orderDir};
            """;

        var items = await connection.QueryAsync<PurchaseReportItem>(sql, parameters);
        var list = items.ToList();

        return new PurchaseReportResult(list, list.Count, list.Sum(x => x.TotalAmount));
    }

    public async Task<PurchaseByProductResult> GetPurchaseByProductAsync(DateRangeFilter dateRange, string? searchTerm, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (dateRange is { FromDate: not null })
        {
            whereClauses.Add("po.OrderDate >= @FromDate");
            parameters.Add("FromDate", dateRange.FromDate.Value);
        }
        if (dateRange is { ToDate: not null })
        {
            whereClauses.Add("po.OrderDate < @ToDate");
            parameters.Add("ToDate", dateRange.ToDate.Value.AddDays(1));
        }
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("(p.Name LIKE @SearchTerm OR p.ProductCode LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        var sql = $"""
            SELECT 
                poi.ProductId,
                p.Name AS ProductName,
                SUM(poi.Quantity) AS TotalQuantity,
                SUM(poi.TotalCost) AS TotalCost,
                COUNT(DISTINCT poi.PurchaseOrderId) AS OrderCount
            FROM PurchaseOrderItems poi
            JOIN PurchaseOrders po ON poi.PurchaseOrderId = po.Id
            JOIN Products p ON poi.ProductId = p.Id
            {whereSql}
            GROUP BY poi.ProductId, p.Name
            ORDER BY TotalCost DESC;
            """;

        var items = await connection.QueryAsync<PurchaseByProductItem>(sql, parameters);
        var list = items.ToList();

        return new PurchaseByProductResult(list, list.Count, list.Sum(x => x.TotalCost));
    }

    public async Task<CustomerReportResult> GetCustomerReportAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (!includeInactive)
        {
            whereClauses.Add("c.Status = 0");
        }
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("(c.FirstName LIKE @SearchTerm OR c.LastName LIKE @SearchTerm OR c.CustomerCode LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        var sql = $"""
            SELECT 
                c.Id AS CustomerId,
                c.CustomerCode,
                c.FirstName || ' ' || c.LastName AS FullName,
                c.PhoneNumber,
                c.Email,
                COUNT(s.Id) AS TotalSales,
                COALESCE(SUM(s.TotalAmount), 0) AS TotalSpent,
                COALESCE(AVG(s.TotalAmount), 0) AS AverageOrderValue,
                MIN(s.SaleDate) AS FirstPurchaseDate,
                MAX(s.SaleDate) AS LastPurchaseDate
            FROM Customers c
            LEFT JOIN Sales s ON c.Id = s.CustomerId AND s.Status = 1
            {whereSql}
            GROUP BY c.Id, c.CustomerCode, c.FirstName, c.LastName, c.PhoneNumber, c.Email
            ORDER BY TotalSpent DESC;
            """;

        var items = await connection.QueryAsync<CustomerReportItem>(sql, parameters);
        var list = items.ToList();

        var activeCount = includeInactive ? list.Count(x => true) : list.Count;
        var inactiveCount = list.Count - activeCount;

        return new CustomerReportResult(list, list.Count, list.Count, 0);
    }

    public async Task<SupplierReportResult> GetSupplierReportAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (!includeInactive)
        {
            whereClauses.Add("s.Status = 0");
        }
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("(s.CompanyName LIKE @SearchTerm OR s.SupplierCode LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        var sql = $"""
            SELECT 
                s.Id AS SupplierId,
                s.SupplierCode,
                s.CompanyName,
                s.ContactPerson,
                s.PhoneNumber,
                COUNT(po.Id) AS TotalOrders,
                COALESCE(SUM(po.TotalAmount), 0) AS TotalPurchases,
                COALESCE(AVG(po.TotalAmount), 0) AS AverageOrderValue,
                MAX(po.OrderDate) AS LastOrderDate
            FROM Suppliers s
            LEFT JOIN PurchaseOrders po ON s.Id = po.SupplierId
            {whereSql}
            GROUP BY s.Id, s.SupplierCode, s.CompanyName, s.ContactPerson, s.PhoneNumber
            ORDER BY TotalPurchases DESC;
            """;

        var items = await connection.QueryAsync<SupplierReportItem>(sql, parameters);
        var list = items.ToList();

        return new SupplierReportResult(list, list.Count, list.Count, 0);
    }

    public async Task<ProductReportResult> GetProductReportAsync(string? searchTerm, string? sortColumn, bool sortDescending, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClauses = new List<string> { "p.IsActive = 1" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("(p.Name LIKE @SearchTerm OR p.ProductCode LIKE @SearchTerm OR c.Name LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        var whereSql = "WHERE " + string.Join(" AND ", whereClauses);

        var orderBy = sortColumn switch
        {
            nameof(ProductReportItem.TotalSold) => "TotalSold",
            nameof(ProductReportItem.TotalRevenue) => "TotalRevenue",
            nameof(ProductReportItem.TotalProfit) => "TotalProfit",
            nameof(ProductReportItem.ProductName) => "p.Name",
            _ => "TotalRevenue"
        };
        var orderDir = sortDescending ? "DESC" : "ASC";

        var sql = $"""
            SELECT 
                p.Id AS ProductId,
                p.ProductCode,
                p.Name AS ProductName,
                c.Name AS CategoryName,
                p.SalePrice,
                p.CostPrice,
                COALESCE(SUM(si.Quantity), 0) AS TotalSold,
                COALESCE(SUM(si.TotalPrice), 0) AS TotalRevenue,
                COALESCE(SUM(si.TotalPrice) - SUM(si.Quantity) * p.CostPrice, 0) AS TotalProfit,
                COALESCE(i.CurrentQuantity, 0) AS StockLevel
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            LEFT JOIN SaleItems si ON p.Id = si.ProductId
            LEFT JOIN Sales s ON si.SaleId = s.Id AND s.Status = 1
            LEFT JOIN InventoryItems i ON p.Id = i.ProductId
            {whereSql}
            GROUP BY p.Id, p.ProductCode, p.Name, c.Name, p.SalePrice, p.CostPrice, i.CurrentQuantity
            ORDER BY {orderBy} {orderDir};
            """;

        var items = await connection.QueryAsync<ProductReportItem>(sql, parameters);
        var list = items.ToList();

        return new ProductReportResult(list, list.Count, list.Count);
    }

    public async Task<FinancialReportResult> GetFinancialReportAsync(DateRangeFilter dateRange, string periodType, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        var fromDate = dateRange?.FromDate ?? DateTime.UtcNow.AddMonths(-12);
        var toDate = dateRange?.ToDate ?? DateTime.UtcNow;

        parameters.Add("FromDate", fromDate);
        parameters.Add("ToDate", toDate.AddDays(1));

        var groupBy = periodType.ToLower() switch
        {
            "week" => "strftime('%Y-%W', s.SaleDate)",
            "month" => "strftime('%Y-%m', s.SaleDate)",
            "year" => "strftime('%Y', s.SaleDate)",
            _ => "strftime('%Y-%m', s.SaleDate)"
        };

        var sql = $"""
            SELECT 
                {groupBy} AS Period,
                MIN(s.SaleDate) AS PeriodStart,
                MAX(s.SaleDate) AS PeriodEnd,
                COALESCE(SUM(s.TotalAmount), 0) AS Revenue,
                COALESCE(SUM(si.Quantity * p.CostPrice), 0) AS CostOfGoodsSold,
                COALESCE(SUM(s.TotalAmount) - SUM(si.Quantity * p.CostPrice), 0) AS GrossProfit,
                COALESCE(SUM(s.TaxAmount), 0) AS SalesTax,
                COALESCE(SUM(s.TotalAmount) - SUM(si.Quantity * p.CostPrice) - SUM(s.TaxAmount), 0) AS NetProfit
            FROM Sales s
            JOIN SaleItems si ON s.Id = si.SaleId
            JOIN Products p ON si.ProductId = p.Id
            WHERE s.Status = 1
              AND s.SaleDate >= @FromDate
              AND s.SaleDate < @ToDate
            GROUP BY {groupBy}
            ORDER BY PeriodStart ASC;
            """;

        var items = await connection.QueryAsync<FinancialReportItem>(sql, parameters);
        var list = items.ToList();

        return new FinancialReportResult(
            list,
            list.Count,
            list.Sum(x => x.Revenue),
            list.Sum(x => x.CostOfGoodsSold),
            list.Sum(x => x.GrossProfit),
            list.Sum(x => x.SalesTax),
            list.Sum(x => x.NetProfit));
    }
}
