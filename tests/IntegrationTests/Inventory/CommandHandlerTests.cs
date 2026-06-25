using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Inventory.Application.CreateInventoryItem;
using RMS.Modules.Inventory.Application.DecreaseStock;
using RMS.Modules.Inventory.Application.GetInventoryItem;
using RMS.Modules.Inventory.Application.GetInventoryPaged;
using RMS.Modules.Inventory.Application.IncreaseStock;
using RMS.Modules.Inventory.Application.AdjustStock;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Inventory.Domain.ValueObjects;
using Xunit;

namespace RMS.IntegrationTests.Inventory;

public class InventoryCommandHandlerTests : InventoryIntegrationTestBase, IClassFixture<InventoryTestDatabaseFixture>
{
    private readonly IMediator _mediator;

    public InventoryCommandHandlerTests(InventoryTestDatabaseFixture fixture) : base(fixture)
    {
        _mediator = fixture.Services.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task CreateInventoryItem_Should_ReturnItemId()
    {
        var command = new CreateInventoryItemCommand(Guid.NewGuid(), 50, 10);
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateInventoryItem_WithDuplicateProductId_Should_Fail()
    {
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(Guid.NewGuid(), productId, 10, 10);
        await WriteStore.InsertAsync(item);

        var command = new CreateInventoryItemCommand(productId, 20, 10);
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Inventory.AlreadyExists");
    }

    [Fact]
    public async Task IncreaseStock_Should_IncreaseQuantity()
    {
        var item = CreateSampleItem(quantity: 10);
        await WriteStore.InsertAsync(item);

        var command = new IncreaseStockCommand(item.Id, 5, "Restock");
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(item.Id);
        updated!.CurrentQuantity.Should().Be(15);
    }

    [Fact]
    public async Task IncreaseStock_WithNonExistentId_Should_Fail()
    {
        var command = new IncreaseStockCommand(Guid.NewGuid(), 5, "Restock");
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Inventory.NotFound");
    }

    [Fact]
    public async Task DecreaseStock_Should_DecreaseQuantity()
    {
        var item = CreateSampleItem(quantity: 20);
        await WriteStore.InsertAsync(item);

        var command = new DecreaseStockCommand(item.Id, 5, "Sale");
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(item.Id);
        updated!.CurrentQuantity.Should().Be(15);
    }

    [Fact]
    public async Task DecreaseStock_BelowZero_Should_Fail()
    {
        var item = CreateSampleItem(quantity: 5);
        await WriteStore.InsertAsync(item);

        var command = new DecreaseStockCommand(item.Id, 10, "Sale");
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Inventory.InsufficientStock");
    }

    [Fact]
    public async Task AdjustStock_Should_SetNewQuantity()
    {
        var item = CreateSampleItem(quantity: 10);
        await WriteStore.InsertAsync(item);

        var command = new AdjustStockCommand(item.Id, 25, "Inventory count");
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(item.Id);
        updated!.CurrentQuantity.Should().Be(25);
    }

    [Fact]
    public async Task GetInventoryItem_Should_ReturnItem()
    {
        var item = CreateSampleItem(quantity: 10);
        await WriteStore.InsertAsync(item);

        var query = new GetInventoryItemQuery(item.Id);
        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task GetInventoryItem_WithNonExistentId_Should_Fail()
    {
        var query = new GetInventoryItemQuery(Guid.NewGuid());
        var result = await _mediator.Send(query);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Inventory.NotFound");
    }

    [Fact]
    public async Task GetInventoryPaged_Should_ReturnPagedResult()
    {
        for (int i = 0; i < 10; i++)
        {
            var item = CreateSampleItem(id: Guid.NewGuid(), quantity: i);
            await WriteStore.InsertAsync(item);
        }

        var query = new GetInventoryPagedQuery(1, 5, null, false);
        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().BeGreaterThanOrEqualTo(10);
    }
}
