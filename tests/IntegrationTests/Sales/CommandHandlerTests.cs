using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Sales.Application.AddSaleItem;
using RMS.Modules.Sales.Application.CompleteSale;
using RMS.Modules.Sales.Application.CreateSale;
using RMS.Modules.Sales.Application.GetSaleById;
using RMS.Modules.Sales.Application.GetSalesPaged;
using RMS.Modules.Sales.Application.RefundSale;
using RMS.Modules.Sales.Application.RemoveSaleItem;
using RMS.Modules.Sales.Domain.Entities;
using Xunit;

namespace RMS.IntegrationTests.Sales;

public class SalesCommandHandlerTests : SalesIntegrationTestBase, IClassFixture<SalesTestDatabaseFixture>
{
    private readonly IMediator _mediator;

    public SalesCommandHandlerTests(SalesTestDatabaseFixture fixture) : base(fixture)
    {
        _mediator = fixture.Services.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task CreateSale_Should_ReturnSaleId()
    {
        var command = new CreateSaleCommand(Guid.NewGuid(), "Test notes");
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task AddSaleItem_Should_AddItem()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        await WriteStore.InsertAsync(sale);

        var command = new AddSaleItemCommand(sale.Id, Guid.NewGuid(), "Gadget", 3, 15.00m);
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(sale.Id);
        updated!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddSaleItem_WithNonExistentSale_Should_Fail()
    {
        var command = new AddSaleItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Gadget", 3, 15.00m);
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Sales.NotFound");
    }

    [Fact]
    public async Task RemoveSaleItem_Should_RemoveItem()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        await WriteStore.InsertAsync(sale);
        var itemId = sale.Items.First().Id;

        var command = new RemoveSaleItemCommand(sale.Id, itemId);
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(sale.Id);
        updated!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CompleteSale_Should_CompleteSale()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        await WriteStore.InsertAsync(sale);

        var command = new CompleteSaleCommand(sale.Id, 10, 20);
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(sale.Id);
        updated!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task CompleteSale_EmptySale_Should_Fail()
    {
        var sale = CreateSampleSale();
        await WriteStore.InsertAsync(sale);

        var command = new CompleteSaleCommand(sale.Id, 0, 0);
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Sale.EmptySale");
    }

    [Fact]
    public async Task RefundSale_Should_RefundSale()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.Complete();
        await WriteStore.InsertAsync(sale);

        var command = new RefundSaleCommand(sale.Id, "Customer request");
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(sale.Id);
        updated!.Status.Should().Be("Refunded");
    }

    [Fact]
    public async Task RefundSale_NotCompleted_Should_Fail()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        await WriteStore.InsertAsync(sale);

        var command = new RefundSaleCommand(sale.Id);
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Sale.NotCompleted");
    }

    [Fact]
    public async Task GetSaleById_Should_ReturnSale()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        await WriteStore.InsertAsync(sale);

        var query = new GetSaleByIdQuery(sale.Id);
        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(sale.Id);
    }

    [Fact]
    public async Task GetSaleById_WithNonExistentId_Should_Fail()
    {
        var query = new GetSaleByIdQuery(Guid.NewGuid());
        var result = await _mediator.Send(query);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Sales.NotFound");
    }

    [Fact]
    public async Task GetSalesPaged_Should_ReturnPagedResult()
    {
        for (int i = 0; i < 10; i++)
        {
            var sale = CreateSampleSale(id: Guid.NewGuid());
            sale.AddItem(Guid.NewGuid(), $"Product {i}", 1, 10.00m + i);
            await WriteStore.InsertAsync(sale);
        }

        var query = new GetSalesPagedQuery(1, 5);
        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().BeGreaterThanOrEqualTo(10);
    }
}
