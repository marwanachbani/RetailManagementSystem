using FluentAssertions;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Inventory.Domain.ValueObjects;
using Xunit;

namespace RMS.IntegrationTests.Inventory;

public class InventoryReadStoreTests : InventoryIntegrationTestBase, IClassFixture<InventoryTestDatabaseFixture>
{
    public InventoryReadStoreTests(InventoryTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetByIdAsync_WhenItemExists_Should_ReturnItem()
    {
        var item = CreateSampleItem();
        await WriteStore.InsertAsync(item);

        var result = await ReadStore.GetByIdAsync(item.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
        result.CurrentQuantity.Should().Be(item.CurrentQuantity.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemNotExists_Should_ReturnNull()
    {
        var result = await ReadStore.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProductIdAsync_WhenItemExists_Should_ReturnItem()
    {
        var productId = Guid.NewGuid();
        var item = CreateSampleItem(productId: productId);
        await WriteStore.InsertAsync(item);

        var result = await ReadStore.GetByProductIdAsync(productId);

        result.Should().NotBeNull();
        result!.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task GetLowStockItemsAsync_Should_ReturnItemsBelowThreshold()
    {
        var item1 = CreateSampleItem(quantity: 5);  // below threshold of 10
        var item2 = CreateSampleItem(quantity: 20); // above threshold
        await WriteStore.InsertAsync(item1);
        await WriteStore.InsertAsync(item2);

        var results = await ReadStore.GetLowStockItemsAsync(10);

        results.Should().ContainSingle();
        results[0].Id.Should().Be(item1.Id);
    }

    [Fact]
    public async Task GetHistoryAsync_Should_ReturnTransactions()
    {
        var item = CreateSampleItem(quantity: 10);
        await WriteStore.InsertAsync(item);
        item.ClearDomainEvents();

        item.IncreaseStock(5, "Restock");
        await WriteStore.UpdateAsync(item);

        var history = await ReadStore.GetHistoryAsync(item.Id);

        history.Should().ContainSingle();
        history[0].ChangeAmount.Should().Be(5);
        history[0].Reason.Should().Be("Restock");
    }

    [Fact]
    public async Task GetPagedAsync_Should_ReturnPagedResult()
    {
        for (int i = 0; i < 5; i++)
        {
            var item = CreateSampleItem(id: Guid.NewGuid(), quantity: i);
            await WriteStore.InsertAsync(item);
        }

        var page = await ReadStore.GetPagedAsync(1, 2, null, false);

        page.Items.Should().HaveCount(2);
        page.PageNumber.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.TotalCount.Should().Be(5);
        page.TotalPages.Should().Be(3);
    }
}
