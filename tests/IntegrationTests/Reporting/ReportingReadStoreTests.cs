using FluentAssertions;
using RMS.Modules.Reporting.Application.Contracts;
using Xunit;

namespace RMS.IntegrationTests.Reporting;

public class ReportingReadStoreTests : ReportingTestBase, IClassFixture<ReportingTestDatabaseFixture>
{
    public ReportingReadStoreTests(ReportingTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetSalesReportAsync_WhenNoSales_Should_ReturnEmpty()
    {
        var result = await ReadStore.GetSalesReportAsync(new DateRangeFilter(null, null), null, null, false);

        result.TotalCount.Should().Be(0);
        result.GrandTotalRevenue.Should().Be(0);
    }

    [Fact]
    public async Task GetInventoryReportAsync_WhenNoInventory_Should_ReturnEmpty()
    {
        var result = await ReadStore.GetInventoryReportAsync(null);

        result.TotalCount.Should().Be(0);
        result.TotalInventoryValue.Should().Be(0);
    }

    [Fact]
    public async Task GetCustomerReportAsync_WhenNoCustomers_Should_ReturnEmpty()
    {
        var result = await ReadStore.GetCustomerReportAsync(null, true);

        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetSupplierReportAsync_WhenNoSuppliers_Should_ReturnEmpty()
    {
        var result = await ReadStore.GetSupplierReportAsync(null, true);

        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetProductReportAsync_WhenNoProducts_Should_ReturnEmpty()
    {
        var result = await ReadStore.GetProductReportAsync(null, null, false);

        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetFinancialReportAsync_WhenNoSales_Should_ReturnEmpty()
    {
        var result = await ReadStore.GetFinancialReportAsync(new DateRangeFilter(null, null), "monthly");

        result.TotalPeriods.Should().Be(0);
        result.TotalRevenue.Should().Be(0);
    }
}
