using FluentAssertions;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Sales.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RMS.IntegrationTests.Sales;

public class SaleWriteStoreTests : SalesIntegrationTestBase, IClassFixture<SalesTestDatabaseFixture>
{
    public SaleWriteStoreTests(SalesTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task InsertAsync_Should_PersistSale()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);

        await WriteStore.InsertAsync(sale);

        var result = await ReadStore.GetByIdAsync(sale.Id);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task InsertAsync_Should_PersistDomainEvents()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        var eventStore = Fixture.Services.GetRequiredService<IEventStore>();

        await WriteStore.InsertAsync(sale);

        var events = await eventStore.GetByAggregateIdAsync(sale.Id);
        events.Should().Contain(e => e.EventType.Contains("SaleCreatedEvent"));
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifySale()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        await WriteStore.InsertAsync(sale);
        sale.ClearDomainEvents();

        sale.Complete();
        await WriteStore.UpdateAsync(sale);

        var result = await ReadStore.GetByIdAsync(sale.Id);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task InsertReceiptAsync_Should_PersistReceipt()
    {
        var sale = CreateSampleSale();
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        await WriteStore.InsertAsync(sale);

        var receipt = Receipt.Create(Guid.NewGuid(), sale.Id, "REC-001", "Test Store", "Cashier", sale.TotalAmount);
        await WriteStore.InsertReceiptAsync(receipt);

        var result = await ReadStore.GetReceiptBySaleIdAsync(sale.Id);
        result.Should().NotBeNull();
        result!.ReceiptNumber.Should().Be("REC-001");
    }
}
