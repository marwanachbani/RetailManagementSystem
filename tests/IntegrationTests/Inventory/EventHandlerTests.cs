using Dapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.EventBus;
using RMS.Modules.Inventory.Application.Contracts;
using RMS.Modules.Inventory.Application.EventHandlers;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Products.Application;
using RMS.BuildingBlocks.Contracts;
using Xunit;

namespace RMS.IntegrationTests.Inventory;

public class InventoryEventHandlerTests : InventoryIntegrationTestBase, IClassFixture<InventoryTestDatabaseFixture>
{
    public InventoryEventHandlerTests(InventoryTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ProductCreatedEventHandler_Should_CreateInventoryItem()
    {
        var handler = Fixture.Services.GetRequiredService<IIntegrationEventHandler<ProductCreatedIntegrationEvent>>();
        var productId = Guid.NewGuid();
        var evt = new ProductCreatedIntegrationEvent(productId, "PRD-001", "Test Product");

        await handler.HandleAsync(evt);

        var item = await ReadStore.GetByProductIdAsync(productId);
        item.Should().NotBeNull();
        item!.ProductId.Should().Be(productId);
        item.CurrentQuantity.Should().Be(0);
    }

    [Fact]
    public async Task ProductCreatedEventHandler_WhenItemAlreadyExists_Should_NotCreateDuplicate()
    {
        var productId = Guid.NewGuid();
        var existingItem = InventoryItem.Create(Guid.NewGuid(), productId, 50, 10);
        await WriteStore.InsertAsync(existingItem);

        var handler = Fixture.Services.GetRequiredService<IIntegrationEventHandler<ProductCreatedIntegrationEvent>>();
        var evt = new ProductCreatedIntegrationEvent(productId, "PRD-002", "Test Product");
        await handler.HandleAsync(evt);

        using var connection = Fixture.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM InventoryItems WHERE ProductId = @ProductId",
            new { ProductId = productId });
        count.Should().Be(1);
    }

    [Fact]
    public async Task ProductDeactivatedEventHandler_Should_DeactivateInventoryItem()
    {
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(Guid.NewGuid(), productId, 50, 10);
        await WriteStore.InsertAsync(item);

        var handler = Fixture.Services.GetRequiredService<IIntegrationEventHandler<ProductDeactivatedIntegrationEvent>>();
        var evt = new ProductDeactivatedIntegrationEvent(productId, "PRD-003");
        await handler.HandleAsync(evt);

        var updated = await ReadStore.GetByProductIdAsync(productId);
        updated.Should().NotBeNull();
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ProductDeactivatedEventHandler_WhenItemNotFound_Should_NotThrow()
    {
        var handler = Fixture.Services.GetRequiredService<IIntegrationEventHandler<ProductDeactivatedIntegrationEvent>>();
        var evt = new ProductDeactivatedIntegrationEvent(Guid.NewGuid(), "PRD-004");

        var act = async () => await handler.HandleAsync(evt);
        await act.Should().NotThrowAsync();
    }
}
