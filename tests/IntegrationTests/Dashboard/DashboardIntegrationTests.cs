using Dapper;
using FluentAssertions;
using Xunit;

namespace RMS.IntegrationTests.Dashboard;

public class DashboardIntegrationTests : IClassFixture<DashboardTestDatabaseFixture>
{
    private readonly DashboardTestDatabaseFixture _fixture;

    public DashboardIntegrationTests(DashboardTestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    [Fact]
    public async Task GetDashboardSummary_WhenEmpty_ShouldReturnZeroes()
    {
        using var connection = _fixture.ConnectionFactory.CreateConnection();
        const string sql = """
            SELECT
                (SELECT COUNT(1) FROM Sales WHERE date(SaleDate) = date(@Today) AND Status = 1) AS TodaysSales,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE date(SaleDate) = date(@Today) AND Status = 1), 0) AS TodaysRevenue,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1), 0) AS MonthlyRevenue,
                (SELECT COUNT(1) FROM Products) AS TotalProducts,
                (SELECT COUNT(1) FROM Customers WHERE Status = 0) AS ActiveCustomers,
                (SELECT COUNT(1) FROM Suppliers WHERE Status = 0) AS ActiveSuppliers,
                (SELECT COUNT(1) FROM PurchaseOrders WHERE date(OrderDate) = date(@Today)) AS PurchaseOrdersToday,
                (SELECT COUNT(1) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.CurrentQuantity <= i.LowStockThreshold AND i.CurrentQuantity > 0 AND i.IsActive = 1) AS LowStockProducts,
                (SELECT COUNT(1) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.CurrentQuantity = 0 AND i.IsActive = 1) AS OutOfStockProducts,
                COALESCE((SELECT SUM(p.CostPrice * i.CurrentQuantity) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.IsActive = 1), 0) AS InventoryValue;
            """;

        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = await connection.QueryFirstAsync<DashboardSummaryDto>(sql, new { Today = today, FirstDayOfMonth = firstDayOfMonth });

        result.TodaysSales.Should().Be(0);
        result.TodaysRevenue.Should().Be(0);
        result.MonthlyRevenue.Should().Be(0);
        result.TotalProducts.Should().Be(0);
        result.ActiveCustomers.Should().Be(0);
        result.ActiveSuppliers.Should().Be(0);
        result.PurchaseOrdersToday.Should().Be(0);
        result.LowStockProducts.Should().Be(0);
        result.OutOfStockProducts.Should().Be(0);
        result.InventoryValue.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardSummary_WithData_ShouldReturnCorrectCounts()
    {
        using var connection = _fixture.ConnectionFactory.CreateConnection();

        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        connection.Execute("INSERT INTO Categories (Id, Name) VALUES (@Id, @Name);",
            new { Id = categoryId, Name = "Electronics" });

        connection.Execute("INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt) VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt);",
            new { Id = productId, ProductCode = "PRD-001", Name = "Widget A", Description = "Test", Barcode = "BAR001", CategoryId = categoryId, SalePrice = 10m, CostPrice = 5m, IsActive = 1, CreatedAt = DateTime.UtcNow });
        connection.Execute("INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt) VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt);",
            new { Id = productId2, ProductCode = "PRD-002", Name = "Widget B", Description = "Test", Barcode = "BAR002", CategoryId = categoryId, SalePrice = 20m, CostPrice = 8m, IsActive = 1, CreatedAt = DateTime.UtcNow });

        connection.Execute("INSERT INTO InventoryItems (Id, ProductId, CurrentQuantity, IsActive, CreatedAt, LowStockThreshold) VALUES (@Id, @ProductId, @CurrentQuantity, @IsActive, @CreatedAt, @LowStockThreshold);",
            new { Id = Guid.NewGuid(), ProductId = productId, CurrentQuantity = 5, IsActive = 1, CreatedAt = DateTime.UtcNow, LowStockThreshold = 10 });
        connection.Execute("INSERT INTO InventoryItems (Id, ProductId, CurrentQuantity, IsActive, CreatedAt, LowStockThreshold) VALUES (@Id, @ProductId, @CurrentQuantity, @IsActive, @CreatedAt, @LowStockThreshold);",
            new { Id = Guid.NewGuid(), ProductId = productId2, CurrentQuantity = 0, IsActive = 1, CreatedAt = DateTime.UtcNow, LowStockThreshold = 10 });

        connection.Execute("INSERT INTO Customers (Id, CustomerCode, FirstName, LastName, PhoneNumber, Email, Status, CreatedAt) VALUES (@Id, @CustomerCode, @FirstName, @LastName, @PhoneNumber, @Email, @Status, @CreatedAt);",
            new { Id = customerId, CustomerCode = "CUST-001", FirstName = "John", LastName = "Doe", PhoneNumber = "+1234567890", Email = "john@test.com", Status = 0, CreatedAt = DateTime.UtcNow });

        connection.Execute("INSERT INTO Suppliers (Id, SupplierCode, CompanyName, PhoneNumber, Email, Status, CreatedAt) VALUES (@Id, @SupplierCode, @CompanyName, @PhoneNumber, @Email, @Status, @CreatedAt);",
            new { Id = supplierId, SupplierCode = "SUPP-001", CompanyName = "Acme Corp", PhoneNumber = "+0987654321", Email = "acme@test.com", Status = 0, CreatedAt = DateTime.UtcNow });

        var saleId = Guid.NewGuid();
        connection.Execute("INSERT INTO Sales (Id, SaleNumber, CashierId, SaleDate, Status, SubTotal, DiscountAmount, TaxAmount, TotalAmount, DiscountPercentage, TaxPercentage, CreatedAt) VALUES (@Id, @SaleNumber, @CashierId, @SaleDate, @Status, @SubTotal, @DiscountAmount, @TaxAmount, @TotalAmount, @DiscountPercentage, @TaxPercentage, @CreatedAt);",
            new { Id = saleId, SaleNumber = "SALE-001", CashierId = Guid.NewGuid(), SaleDate = DateTime.UtcNow, Status = 1, SubTotal = 100m, DiscountAmount = 0, TaxAmount = 0, TotalAmount = 100m, DiscountPercentage = 0, TaxPercentage = 0, CreatedAt = DateTime.UtcNow });

        connection.Execute("INSERT INTO PurchaseOrders (Id, PurchaseNumber, SupplierId, SupplierName, OrderDate, Status, SubTotal, TaxAmount, TotalAmount, TaxPercentage, CreatedAt) VALUES (@Id, @PurchaseNumber, @SupplierId, @SupplierName, @OrderDate, @Status, @SubTotal, @TaxAmount, @TotalAmount, @TaxPercentage, @CreatedAt);",
            new { Id = Guid.NewGuid(), PurchaseNumber = "PO-001", SupplierId = supplierId, SupplierName = "Acme Corp", OrderDate = DateTime.UtcNow, Status = 1, SubTotal = 500m, TaxAmount = 0, TotalAmount = 500m, TaxPercentage = 0, CreatedAt = DateTime.UtcNow });

        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstDayOfMonth2 = firstDayOfMonth;

        const string sql = """
            SELECT
                (SELECT COUNT(1) FROM Sales WHERE date(SaleDate) = date(@Today) AND Status = 1) AS TodaysSales,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE date(SaleDate) = date(@Today) AND Status = 1), 0) AS TodaysRevenue,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1), 0) AS MonthlyRevenue,
                (SELECT COUNT(1) FROM Products) AS TotalProducts,
                (SELECT COUNT(1) FROM Customers WHERE Status = 0) AS ActiveCustomers,
                (SELECT COUNT(1) FROM Suppliers WHERE Status = 0) AS ActiveSuppliers,
                (SELECT COUNT(1) FROM PurchaseOrders WHERE date(OrderDate) = date(@Today)) AS PurchaseOrdersToday,
                (SELECT COUNT(1) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.CurrentQuantity <= i.LowStockThreshold AND i.CurrentQuantity > 0 AND i.IsActive = 1) AS LowStockProducts,
                (SELECT COUNT(1) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.CurrentQuantity = 0 AND i.IsActive = 1) AS OutOfStockProducts,
                COALESCE((SELECT SUM(p.CostPrice * i.CurrentQuantity) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.IsActive = 1), 0) AS InventoryValue;
            """;

        var result = await connection.QueryFirstAsync<DashboardSummaryDto>(sql, new { Today = today, FirstDayOfMonth = firstDayOfMonth2 });

        result.TodaysSales.Should().Be(1);
        result.TodaysRevenue.Should().Be(100);
        result.MonthlyRevenue.Should().Be(100);
        result.TotalProducts.Should().Be(2);
        result.ActiveCustomers.Should().Be(1);
        result.ActiveSuppliers.Should().Be(1);
        result.PurchaseOrdersToday.Should().Be(1);
        result.LowStockProducts.Should().Be(1);
        result.OutOfStockProducts.Should().Be(1);
        result.InventoryValue.Should().Be(25);
    }

    [Fact]
    public async Task GetRecentSales_WithData_ShouldReturnSalesOrderedByDate()
    {
        using var connection = _fixture.ConnectionFactory.CreateConnection();

        var saleId1 = Guid.NewGuid();
        var saleId2 = Guid.NewGuid();

        connection.Execute("INSERT INTO Sales (Id, SaleNumber, CashierId, SaleDate, Status, SubTotal, DiscountAmount, TaxAmount, TotalAmount, DiscountPercentage, TaxPercentage, CreatedAt) VALUES (@Id, @SaleNumber, @CashierId, @SaleDate, @Status, @SubTotal, @DiscountAmount, @TaxAmount, @TotalAmount, @DiscountPercentage, @TaxPercentage, @CreatedAt);",
            new { Id = saleId1, SaleNumber = "SALE-OLD", CashierId = Guid.NewGuid(), SaleDate = DateTime.UtcNow.AddDays(-2), Status = 1, SubTotal = 50m, DiscountAmount = 0, TaxAmount = 0, TotalAmount = 50m, DiscountPercentage = 0, TaxPercentage = 0, CreatedAt = DateTime.UtcNow.AddDays(-2) });
        connection.Execute("INSERT INTO Sales (Id, SaleNumber, CashierId, SaleDate, Status, SubTotal, DiscountAmount, TaxAmount, TotalAmount, DiscountPercentage, TaxPercentage, CreatedAt) VALUES (@Id, @SaleNumber, @CashierId, @SaleDate, @Status, @SubTotal, @DiscountAmount, @TaxAmount, @TotalAmount, @DiscountPercentage, @TaxPercentage, @CreatedAt);",
            new { Id = saleId2, SaleNumber = "SALE-NEW", CashierId = Guid.NewGuid(), SaleDate = DateTime.UtcNow, Status = 1, SubTotal = 75m, DiscountAmount = 0, TaxAmount = 0, TotalAmount = 75m, DiscountPercentage = 0, TaxPercentage = 0, CreatedAt = DateTime.UtcNow });

        const string sql = """
            SELECT Id, SaleNumber, TotalAmount, SaleDate,
                   CASE Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Refunded' END AS Status
            FROM Sales
            ORDER BY CreatedAt DESC
            LIMIT @Limit;
            """;

        var rows = await connection.QueryAsync<RecentSaleDto>(sql, new { Limit = 5 });
        var list = rows.ToList();

        list.Should().HaveCount(2);
        list[0].SaleNumber.Should().Be("SALE-NEW");
        list[0].TotalAmount.Should().Be(75);
        list[0].Status.Should().Be("Completed");
        list[1].SaleNumber.Should().Be("SALE-OLD");
        list[1].TotalAmount.Should().Be(50);
    }

    [Fact]
    public async Task GetRecentPurchases_WithData_ShouldReturnOrdersOrderedByDate()
    {
        using var connection = _fixture.ConnectionFactory.CreateConnection();

        var supplierId = Guid.NewGuid();
        connection.Execute("INSERT INTO Suppliers (Id, SupplierCode, CompanyName, PhoneNumber, Status, CreatedAt) VALUES (@Id, @SupplierCode, @CompanyName, @PhoneNumber, @Status, @CreatedAt);",
            new { Id = supplierId, SupplierCode = "SUPP-001", CompanyName = "Test Supplier", PhoneNumber = "+1234567890", Status = 0, CreatedAt = DateTime.UtcNow });

        var poId1 = Guid.NewGuid();
        var poId2 = Guid.NewGuid();

        connection.Execute("INSERT INTO PurchaseOrders (Id, PurchaseNumber, SupplierId, SupplierName, OrderDate, Status, SubTotal, TaxAmount, TotalAmount, TaxPercentage, CreatedAt) VALUES (@Id, @PurchaseNumber, @SupplierId, @SupplierName, @OrderDate, @Status, @SubTotal, @TaxAmount, @TotalAmount, @TaxPercentage, @CreatedAt);",
            new { Id = poId1, PurchaseNumber = "PO-OLD", SupplierId = supplierId, SupplierName = "Test Supplier", OrderDate = DateTime.UtcNow.AddDays(-3), Status = 1, SubTotal = 200m, TaxAmount = 0, TotalAmount = 200m, TaxPercentage = 0, CreatedAt = DateTime.UtcNow.AddDays(-3) });
        connection.Execute("INSERT INTO PurchaseOrders (Id, PurchaseNumber, SupplierId, SupplierName, OrderDate, Status, SubTotal, TaxAmount, TotalAmount, TaxPercentage, CreatedAt) VALUES (@Id, @PurchaseNumber, @SupplierId, @SupplierName, @OrderDate, @Status, @SubTotal, @TaxAmount, @TotalAmount, @TaxPercentage, @CreatedAt);",
            new { Id = poId2, PurchaseNumber = "PO-NEW", SupplierId = supplierId, SupplierName = "Test Supplier", OrderDate = DateTime.UtcNow, Status = 1, SubTotal = 300m, TaxAmount = 0, TotalAmount = 300m, TaxPercentage = 0, CreatedAt = DateTime.UtcNow });

        const string sql = """
            SELECT Id, PurchaseNumber, SupplierName, TotalAmount, OrderDate,
                   CASE Status WHEN 0 THEN 'Draft' WHEN 1 THEN 'Submitted' WHEN 2 THEN 'PartiallyReceived' WHEN 3 THEN 'Completed' WHEN 4 THEN 'Cancelled' END AS Status
            FROM PurchaseOrders
            ORDER BY CreatedAt DESC
            LIMIT @Limit;
            """;

        var rows = await connection.QueryAsync<RecentPurchaseDto>(sql, new { Limit = 5 });
        var list = rows.ToList();

        list.Should().HaveCount(2);
        list[0].PurchaseNumber.Should().Be("PO-NEW");
        list[0].TotalAmount.Should().Be(300);
        list[0].SupplierName.Should().Be("Test Supplier");
        list[1].PurchaseNumber.Should().Be("PO-OLD");
        list[1].TotalAmount.Should().Be(200);
    }

    [Fact]
    public async Task GetLowStockProducts_WithData_ShouldReturnProductsBelowThreshold()
    {
        using var connection = _fixture.ConnectionFactory.CreateConnection();

        var categoryId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var productId3 = Guid.NewGuid();

        connection.Execute("INSERT INTO Categories (Id, Name) VALUES (@Id, @Name);",
            new { Id = categoryId, Name = "Electronics" });

        connection.Execute("INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt) VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt);",
            new { Id = productId1, ProductCode = "PRD-001", Name = "Widget A", Description = "", Barcode = "BAR001", CategoryId = categoryId, SalePrice = 10m, CostPrice = 5m, IsActive = 1, CreatedAt = DateTime.UtcNow });
        connection.Execute("INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt) VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt);",
            new { Id = productId2, ProductCode = "PRD-002", Name = "Widget B", Description = "", Barcode = "BAR002", CategoryId = categoryId, SalePrice = 20m, CostPrice = 8m, IsActive = 1, CreatedAt = DateTime.UtcNow });
        connection.Execute("INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt) VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt);",
            new { Id = productId3, ProductCode = "PRD-003", Name = "Widget C", Description = "", Barcode = "BAR003", CategoryId = categoryId, SalePrice = 30m, CostPrice = 12m, IsActive = 1, CreatedAt = DateTime.UtcNow });

        connection.Execute("INSERT INTO InventoryItems (Id, ProductId, CurrentQuantity, IsActive, CreatedAt, LowStockThreshold) VALUES (@Id, @ProductId, @CurrentQuantity, @IsActive, @CreatedAt, @LowStockThreshold);",
            new { Id = Guid.NewGuid(), ProductId = productId1, CurrentQuantity = 3, IsActive = 1, CreatedAt = DateTime.UtcNow, LowStockThreshold = 10 });
        connection.Execute("INSERT INTO InventoryItems (Id, ProductId, CurrentQuantity, IsActive, CreatedAt, LowStockThreshold) VALUES (@Id, @ProductId, @CurrentQuantity, @IsActive, @CreatedAt, @LowStockThreshold);",
            new { Id = Guid.NewGuid(), ProductId = productId2, CurrentQuantity = 0, IsActive = 1, CreatedAt = DateTime.UtcNow, LowStockThreshold = 10 });
        connection.Execute("INSERT INTO InventoryItems (Id, ProductId, CurrentQuantity, IsActive, CreatedAt, LowStockThreshold) VALUES (@Id, @ProductId, @CurrentQuantity, @IsActive, @CreatedAt, @LowStockThreshold);",
            new { Id = Guid.NewGuid(), ProductId = productId3, CurrentQuantity = 100, IsActive = 1, CreatedAt = DateTime.UtcNow, LowStockThreshold = 50 });

        const string sql = """
            SELECT p.Id AS ProductId, p.Name AS ProductName, i.CurrentQuantity, i.LowStockThreshold
            FROM InventoryItems i
            JOIN Products p ON i.ProductId = p.Id
            WHERE i.CurrentQuantity <= i.LowStockThreshold AND i.IsActive = 1
            ORDER BY i.CurrentQuantity ASC
            LIMIT @Limit;
            """;

        var rows = await connection.QueryAsync<LowStockProductDto>(sql, new { Limit = 10 });
        var list = rows.ToList();

        list.Should().HaveCount(2);
        list[0].ProductName.Should().Be("Widget B");
        list[0].CurrentQuantity.Should().Be(0);
        list[1].ProductName.Should().Be("Widget A");
        list[1].CurrentQuantity.Should().Be(3);
    }

    [Fact]
    public async Task GetRecentActivities_WithData_ShouldReturnUnifiedActivityFeed()
    {
        using var connection = _fixture.ConnectionFactory.CreateConnection();

        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        connection.Execute("INSERT INTO Categories (Id, Name) VALUES (@Id, @Name);",
            new { Id = categoryId, Name = "Electronics" });

        connection.Execute("INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt) VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt);",
            new { Id = productId, ProductCode = "PRD-001", Name = "New Product", Description = "", Barcode = "BAR001", CategoryId = categoryId, SalePrice = 10m, CostPrice = 5m, IsActive = 1, CreatedAt = DateTime.UtcNow });

        connection.Execute("INSERT INTO Customers (Id, CustomerCode, FirstName, LastName, PhoneNumber, Status, CreatedAt) VALUES (@Id, @CustomerCode, @FirstName, @LastName, @PhoneNumber, @Status, @CreatedAt);",
            new { Id = customerId, CustomerCode = "CUST-001", FirstName = "Jane", LastName = "Smith", PhoneNumber = "+1234567890", Status = 0, CreatedAt = DateTime.UtcNow });

        connection.Execute("INSERT INTO Sales (Id, SaleNumber, CashierId, SaleDate, Status, SubTotal, DiscountAmount, TaxAmount, TotalAmount, DiscountPercentage, TaxPercentage, CreatedAt) VALUES (@Id, @SaleNumber, @CashierId, @SaleDate, @Status, @SubTotal, @DiscountAmount, @TaxAmount, @TotalAmount, @DiscountPercentage, @TaxPercentage, @CreatedAt);",
            new { Id = Guid.NewGuid(), SaleNumber = "SALE-001", CashierId = Guid.NewGuid(), SaleDate = DateTime.UtcNow, Status = 1, SubTotal = 25m, DiscountAmount = 0, TaxAmount = 0, TotalAmount = 25m, DiscountPercentage = 0, TaxPercentage = 0, CreatedAt = DateTime.UtcNow });

        var inventoryItemId = Guid.NewGuid();
        connection.Execute("INSERT INTO InventoryItems (Id, ProductId, CurrentQuantity, IsActive, CreatedAt, LowStockThreshold) VALUES (@Id, @ProductId, @CurrentQuantity, @IsActive, @CreatedAt, @LowStockThreshold);",
            new { Id = inventoryItemId, ProductId = productId, CurrentQuantity = 10, IsActive = 1, CreatedAt = DateTime.UtcNow, LowStockThreshold = 10 });
        connection.Execute("INSERT INTO InventoryTransactions (Id, InventoryItemId, ProductId, QuantityBefore, QuantityAfter, ChangeAmount, Reason, UserId, Timestamp) VALUES (@Id, @InventoryItemId, @ProductId, @QuantityBefore, @QuantityAfter, @ChangeAmount, @Reason, @UserId, @Timestamp);",
            new { Id = Guid.NewGuid(), InventoryItemId = inventoryItemId, ProductId = productId, QuantityBefore = 10, QuantityAfter = 15, ChangeAmount = 5, Reason = "Restock", UserId = Guid.NewGuid(), Timestamp = DateTime.UtcNow });

        const string sql = """
            SELECT 'Sale' AS ActivityType,
                   'Sale ' || SaleNumber || ' - ' || printf('%.2f', TotalAmount) AS Description,
                   CreatedAt AS Timestamp,
                   'X' AS IconGlyph
            FROM Sales
            UNION ALL
            SELECT 'Customer' AS ActivityType,
                   'New customer ' || FirstName || ' ' || LastName AS Description,
                   CreatedAt AS Timestamp,
                   'X' AS IconGlyph
            FROM Customers
            UNION ALL
            SELECT 'Product' AS ActivityType,
                   'Product ' || Name || ' added' AS Description,
                   CreatedAt AS Timestamp,
                   'X' AS IconGlyph
            FROM Products
            UNION ALL
            SELECT 'Stock' AS ActivityType,
                   'Stock adjusted for product ' || CAST(ProductId AS TEXT) || ' (' || ChangeAmount || ')' AS Description,
                   Timestamp AS Timestamp,
                   'X' AS IconGlyph
            FROM InventoryTransactions
            ORDER BY Timestamp DESC
            LIMIT @Limit;
            """;

        var rows = await connection.QueryAsync<ActivityDto>(sql, new { Limit = 10 });
        var list = rows.ToList();

        list.Should().NotBeEmpty();
        var activityTypes = list.Select(r => r.ActivityType).ToList();
        activityTypes.Should().Contain("Sale");
        activityTypes.Should().Contain("Customer");
        activityTypes.Should().Contain("Product");
        activityTypes.Should().Contain("Stock");
    }

    [Fact]
    public async Task GetQuickStatistics_WithData_ShouldReturnCorrectStats()
    {
        using var connection = _fixture.ConnectionFactory.CreateConnection();

        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var categoryId = Guid.NewGuid();
        connection.Execute("INSERT INTO Categories (Id, Name) VALUES (@Id, @Name);",
            new { Id = categoryId, Name = "Electronics" });

        connection.Execute("INSERT INTO Sales (Id, SaleNumber, CashierId, SaleDate, Status, SubTotal, DiscountAmount, TaxAmount, TotalAmount, DiscountPercentage, TaxPercentage, CreatedAt) VALUES (@Id, @SaleNumber, @CashierId, @SaleDate, @Status, @SubTotal, @DiscountAmount, @TaxAmount, @TotalAmount, @DiscountPercentage, @TaxPercentage, @CreatedAt);",
            new { Id = Guid.NewGuid(), SaleNumber = "SALE-001", CashierId = Guid.NewGuid(), SaleDate = today, Status = 1, SubTotal = 100m, DiscountAmount = 0, TaxAmount = 0, TotalAmount = 100m, DiscountPercentage = 0, TaxPercentage = 0, CreatedAt = today });
        connection.Execute("INSERT INTO Sales (Id, SaleNumber, CashierId, SaleDate, Status, SubTotal, DiscountAmount, TaxAmount, TotalAmount, DiscountPercentage, TaxPercentage, CreatedAt) VALUES (@Id, @SaleNumber, @CashierId, @SaleDate, @Status, @SubTotal, @DiscountAmount, @TaxAmount, @TotalAmount, @DiscountPercentage, @TaxPercentage, @CreatedAt);",
            new { Id = Guid.NewGuid(), SaleNumber = "SALE-002", CashierId = Guid.NewGuid(), SaleDate = today.AddDays(-1), Status = 1, SubTotal = 200m, DiscountAmount = 0, TaxAmount = 0, TotalAmount = 200m, DiscountPercentage = 0, TaxPercentage = 0, CreatedAt = today.AddDays(-1) });

        connection.Execute("INSERT INTO Customers (Id, CustomerCode, FirstName, LastName, PhoneNumber, Status, CreatedAt) VALUES (@Id, @CustomerCode, @FirstName, @LastName, @PhoneNumber, @Status, @CreatedAt);",
            new { Id = Guid.NewGuid(), CustomerCode = "CUST-001", FirstName = "John", LastName = "Doe", PhoneNumber = "+1234567890", Status = 0, CreatedAt = today });

        connection.Execute("INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt) VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt);",
            new { Id = Guid.NewGuid(), ProductCode = "PRD-001", Name = "Widget", Description = "", Barcode = "BAR001", CategoryId = categoryId, SalePrice = 10m, CostPrice = 5m, IsActive = 1, CreatedAt = today });

        const string sql = """
            SELECT
                (SELECT COUNT(1) FROM Sales WHERE SaleDate >= @StartOfWeek AND Status = 1) AS SalesThisWeek,
                (SELECT COUNT(1) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1) AS SalesThisMonth,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @StartOfWeek AND Status = 1), 0) AS RevenueThisWeek,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1), 0) AS RevenueThisMonth,
                (SELECT COUNT(1) FROM Customers WHERE CreatedAt >= @FirstDayOfMonth) AS NewCustomersThisMonth,
                (SELECT COUNT(1) FROM Products WHERE CreatedAt >= @FirstDayOfMonth) AS NewProductsThisMonth;
            """;

        var result = await connection.QueryFirstAsync<QuickStatisticsDto>(sql, new { StartOfWeek = startOfWeek, FirstDayOfMonth = firstDayOfMonth });

        result.SalesThisWeek.Should().Be(2);
        result.SalesThisMonth.Should().Be(2);
        result.RevenueThisWeek.Should().Be(300);
        result.RevenueThisMonth.Should().Be(300);
        result.NewCustomersThisMonth.Should().Be(1);
        result.NewProductsThisMonth.Should().Be(1);
    }

    [Fact]
    public async Task DashboardQueries_ShouldNeverModifyData()
    {
        using var connection = _fixture.ConnectionFactory.CreateConnection();

        var categoryId = Guid.NewGuid();
        connection.Execute("INSERT INTO Categories (Id, Name) VALUES (@Id, @Name);",
            new { Id = categoryId, Name = "Electronics" });
        connection.Execute("INSERT INTO Products (Id, ProductCode, Name, Description, Barcode, CategoryId, SalePrice, CostPrice, IsActive, CreatedAt) VALUES (@Id, @ProductCode, @Name, @Description, @Barcode, @CategoryId, @SalePrice, @CostPrice, @IsActive, @CreatedAt);",
            new { Id = Guid.NewGuid(), ProductCode = "PRD-001", Name = "Widget", Description = "", Barcode = "BAR001", CategoryId = categoryId, SalePrice = 10m, CostPrice = 5m, IsActive = 1, CreatedAt = DateTime.UtcNow });

        var productCountBefore = connection.QueryFirst<int>("SELECT COUNT(1) FROM Products;");
        var saleCountBefore = connection.QueryFirst<int>("SELECT COUNT(1) FROM Sales;");

        var dashboardSql = """
            SELECT
                (SELECT COUNT(1) FROM Sales WHERE date(SaleDate) = date(@Today) AND Status = 1) AS TodaysSales,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE date(SaleDate) = date(@Today) AND Status = 1), 0) AS TodaysRevenue,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1), 0) AS MonthlyRevenue,
                (SELECT COUNT(1) FROM Products) AS TotalProducts,
                (SELECT COUNT(1) FROM Customers WHERE Status = 0) AS ActiveCustomers,
                (SELECT COUNT(1) FROM Suppliers WHERE Status = 0) AS ActiveSuppliers,
                (SELECT COUNT(1) FROM PurchaseOrders WHERE date(OrderDate) = date(@Today)) AS PurchaseOrdersToday,
                (SELECT COUNT(1) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.CurrentQuantity <= i.LowStockThreshold AND i.CurrentQuantity > 0 AND i.IsActive = 1) AS LowStockProducts,
                (SELECT COUNT(1) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.CurrentQuantity = 0 AND i.IsActive = 1) AS OutOfStockProducts,
                COALESCE((SELECT SUM(p.CostPrice * i.CurrentQuantity) FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id WHERE i.IsActive = 1), 0) AS InventoryValue;
            """;

        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

        await connection.QueryFirstAsync<DashboardSummaryDto>(dashboardSql, new { Today = today, FirstDayOfMonth = firstDayOfMonth });

        var recentSalesSql = """
            SELECT Id, SaleNumber, TotalAmount, SaleDate, Status FROM Sales ORDER BY CreatedAt DESC LIMIT @Limit;
            """;
        await connection.QueryAsync<RecentSaleDto>(recentSalesSql, new { Limit = 5 });

        var recentPurchasesSql = """
            SELECT Id, PurchaseNumber, SupplierName, TotalAmount, OrderDate, Status FROM PurchaseOrders ORDER BY CreatedAt DESC LIMIT @Limit;
            """;
        await connection.QueryAsync<RecentPurchaseDto>(recentPurchasesSql, new { Limit = 5 });

        var lowStockSql = """
            SELECT p.Id AS ProductId, p.Name AS ProductName, i.CurrentQuantity, i.LowStockThreshold
            FROM InventoryItems i JOIN Products p ON i.ProductId = p.Id
            WHERE i.CurrentQuantity <= i.LowStockThreshold AND i.IsActive = 1 ORDER BY i.CurrentQuantity ASC LIMIT @Limit;
            """;
        await connection.QueryAsync<LowStockProductDto>(lowStockSql, new { Limit = 10 });

        var activitySql = """
            SELECT 'Sale' AS ActivityType, SaleNumber AS Description, CreatedAt AS Timestamp, 'X' AS IconGlyph FROM Sales
            UNION ALL SELECT 'Purchase' AS ActivityType, PurchaseNumber AS Description, CreatedAt AS Timestamp, 'X' AS IconGlyph FROM PurchaseOrders
            UNION ALL SELECT 'Customer' AS ActivityType, FirstName AS Description, CreatedAt AS Timestamp, 'X' AS IconGlyph FROM Customers
            UNION ALL SELECT 'Product' AS ActivityType, Name AS Description, CreatedAt AS Timestamp, 'X' AS IconGlyph FROM Products
            ORDER BY Timestamp DESC LIMIT @Limit;
            """;
        await connection.QueryAsync<ActivityDto>(activitySql, new { Limit = 10 });

        var statsSql = """
            SELECT
                (SELECT COUNT(1) FROM Sales WHERE SaleDate >= @StartOfWeek AND Status = 1) AS SalesThisWeek,
                (SELECT COUNT(1) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1) AS SalesThisMonth,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @StartOfWeek AND Status = 1), 0) AS RevenueThisWeek,
                COALESCE((SELECT SUM(TotalAmount) FROM Sales WHERE SaleDate >= @FirstDayOfMonth AND Status = 1), 0) AS RevenueThisMonth,
                (SELECT COUNT(1) FROM Customers WHERE CreatedAt >= @FirstDayOfMonth) AS NewCustomersThisMonth,
                (SELECT COUNT(1) FROM Products WHERE CreatedAt >= @FirstDayOfMonth) AS NewProductsThisMonth;
            """;
        await connection.QueryFirstAsync<QuickStatisticsDto>(statsSql, new { StartOfWeek = startOfWeek, FirstDayOfMonth = firstDayOfMonth });

        var productCountAfter = connection.QueryFirst<int>("SELECT COUNT(1) FROM Products;");
        var saleCountAfter = connection.QueryFirst<int>("SELECT COUNT(1) FROM Sales;");

        productCountAfter.Should().Be(productCountBefore);
        saleCountAfter.Should().Be(saleCountBefore);
    }
}

public sealed record DashboardSummaryDto(
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
    private DashboardSummaryDto() : this(0, 0, 0, 0, 0, 0, 0, 0, 0, 0) { }
}

public sealed record RecentSaleDto(Guid Id, string SaleNumber, decimal TotalAmount, DateTime SaleDate, string Status)
{
    private RecentSaleDto() : this(default, "", 0, default, "") { }
}

public sealed record RecentPurchaseDto(Guid Id, string PurchaseNumber, string SupplierName, decimal TotalAmount, DateTime OrderDate, string Status)
{
    private RecentPurchaseDto() : this(default, "", "", 0, default, "") { }
}

public sealed record LowStockProductDto(Guid ProductId, string ProductName, int CurrentQuantity, int LowStockThreshold)
{
    private LowStockProductDto() : this(default, "", 0, 0) { }
}

public sealed record ActivityDto(string ActivityType, string Description, DateTime Timestamp, string IconGlyph)
{
    private ActivityDto() : this("", "", default, "") { }
}

public sealed record QuickStatisticsDto(
    int SalesThisWeek,
    int SalesThisMonth,
    decimal RevenueThisWeek,
    decimal RevenueThisMonth,
    int NewCustomersThisMonth,
    int NewProductsThisMonth)
{
    private QuickStatisticsDto() : this(0, 0, 0, 0, 0, 0) { }
}
