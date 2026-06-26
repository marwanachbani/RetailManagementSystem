using FluentAssertions;
using RMS.Modules.Sales.Domain.Entities;
using Xunit;

namespace RMS.IntegrationTests.Sales;

public class SaleReadStoreTests : SalesIntegrationTestBase, IClassFixture<SalesTestDatabaseFixture>
{
    public SaleReadStoreTests(SalesTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetByIdAsync_WhenSaleExists_Should_ReturnSale()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        await WriteStore.InsertAsync(sale);

        var result = await ReadStore.GetByIdAsync(sale.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSaleNotExists_Should_ReturnNull()
    {
        var result = await ReadStore.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_Should_ReturnPagedResult()
    {
        for (int i = 0; i < 5; i++)
        {
            var sale = CreateSampleSale(id: Guid.NewGuid());
            sale.AddItem(Guid.NewGuid(), $"Product {i}", 1, 10.00m + i);
            await WriteStore.InsertAsync(sale);
        }

        var page = await ReadStore.GetPagedAsync(1, 2, null, null);

        page.Items.Should().HaveCount(2);
        page.PageNumber.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetByDateAsync_Should_ReturnSalesForDate()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        await WriteStore.InsertAsync(sale);

        var results = await ReadStore.GetByDateAsync(DateTime.UtcNow.Date);

        results.Should().ContainSingle();
    }

    [Fact]
    public async Task GetDailySummaryAsync_Should_ReturnSummary()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.Complete();
        await WriteStore.InsertAsync(sale);

        var summary = await ReadStore.GetDailySummaryAsync(DateTime.UtcNow.Date);

        summary.TotalSales.Should().Be(1);
        summary.TotalRevenue.Should().Be(20.00m);
    }
}
