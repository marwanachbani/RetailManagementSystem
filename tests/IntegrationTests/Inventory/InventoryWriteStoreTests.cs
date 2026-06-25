using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Inventory.Domain.ValueObjects;
using Xunit;

namespace RMS.IntegrationTests.Inventory;

public class InventoryWriteStoreTests : InventoryIntegrationTestBase, IClassFixture<InventoryTestDatabaseFixture>
{
    public InventoryWriteStoreTests(InventoryTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task InsertAsync_Should_PersistItem()
    {
        var item = CreateSampleItem();

        await WriteStore.InsertAsync(item);

        var result = await ReadStore.GetByIdAsync(item.Id);
        result.Should().NotBeNull();
        result!.CurrentQuantity.Should().Be(item.CurrentQuantity.Value);
    }

    [Fact]
    public async Task InsertAsync_Should_PersistDomainEvents()
    {
        var item = CreateSampleItem();
        var eventStore = Fixture.Services.GetRequiredService<IEventStore>();

        await WriteStore.InsertAsync(item);

        var events = await eventStore.GetByAggregateIdAsync(item.Id);
        events.Should().Contain(e => e.EventType.Contains("InventoryCreatedEvent"));
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyItem()
    {
        var item = CreateSampleItem(quantity: 10);
        await WriteStore.InsertAsync(item);
        item.ClearDomainEvents();

        item.IncreaseStock(5, "Restock");
        await WriteStore.UpdateAsync(item);

        var result = await ReadStore.GetByIdAsync(item.Id);
        result.Should().NotBeNull();
        result!.CurrentQuantity.Should().Be(15);
    }

    [Fact]
    public async Task UpdateAsync_Should_PersistTransactions()
    {
        var item = CreateSampleItem(quantity: 10);
        await WriteStore.InsertAsync(item);
        item.ClearDomainEvents();

        item.IncreaseStock(5, "Restock");
        await WriteStore.UpdateAsync(item);

        var history = await ReadStore.GetHistoryAsync(item.Id);
        history.Should().ContainSingle();
        history[0].ChangeAmount.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_Should_PersistUpdateEvent()
    {
        var item = CreateSampleItem(quantity: 10);
        await WriteStore.InsertAsync(item);
        item.ClearDomainEvents();

        item.DecreaseStock(3, "Sale");
        var eventStore = Fixture.Services.GetRequiredService<IEventStore>();
        await WriteStore.UpdateAsync(item);

        var events = await eventStore.GetByAggregateIdAsync(item.Id);
        events.Should().Contain(e => e.EventType.Contains("StockDecreasedEvent"));
    }

    [Fact]
    public async Task InsertTransactionAsync_Should_PersistTransaction()
    {
        var item = CreateSampleItem(quantity: 10);
        await WriteStore.InsertAsync(item);
        item.ClearDomainEvents();

        var transaction = InventoryTransaction.Create(
            Guid.NewGuid(), item.Id, item.ProductId, 10, 15, 5, "Test");

        await WriteStore.InsertTransactionAsync(transaction);

        var history = await ReadStore.GetHistoryAsync(item.Id);
        history.Should().ContainSingle();
        history[0].Id.Should().Be(transaction.Id);
    }
}
