using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Products.Domain.Entities;
using RMS.Modules.Products.Domain.ValueObjects;
using RMS.Modules.Products.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Products;

public class ProductWriteStoreTests : ProductsIntegrationTestBase, IClassFixture<TestDatabaseFixture>
{
    public ProductWriteStoreTests(TestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task InsertAsync_Should_PersistProduct()
    {
        var product = CreateSampleProduct();

        await WriteStore.InsertAsync(product);

        var result = await ReadStore.GetByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Sample Product");
    }

    [Fact]
    public async Task InsertAsync_Should_PersistDomainEvents()
    {
        var product = CreateSampleProduct();
        var eventStore = Fixture.Services.GetRequiredService<IEventStore>();

        await WriteStore.InsertAsync(product);

        var events = await eventStore.GetByAggregateIdAsync(product.Id);
        events.Should().ContainSingle();
        events[0].EventType.Should().Contain("ProductCreatedEvent");
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyProduct()
    {
        var product = CreateSampleProduct();
        await WriteStore.InsertAsync(product);
        product.ClearDomainEvents();

        var newBarcode = $"UPD-{Guid.NewGuid():N}";
        product.Update(
            "Updated Name",
            "Updated Description",
            Barcode.Create(newBarcode),
            CreateProductsTablesMigration.ClothingCategoryId,
            Money.Create(200),
            Money.Create(100));

        await WriteStore.UpdateAsync(product);

        var result = await ReadStore.GetByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Barcode.Should().Be(newBarcode);
        result.CategoryId.Should().Be(CreateProductsTablesMigration.ClothingCategoryId);
        result.SalePrice.Should().Be(200);
        result.CostPrice.Should().Be(100);
    }

    [Fact]
    public async Task UpdateAsync_Should_PersistUpdateEvent()
    {
        var product = CreateSampleProduct();
        await WriteStore.InsertAsync(product);
        product.ClearDomainEvents();

        var newBarcode = $"EV-{Guid.NewGuid():N}";
        product.Update(
            "Updated Name",
            null,
            Barcode.Create(newBarcode),
            CreateProductsTablesMigration.ElectronicsCategoryId,
            Money.Create(100),
            Money.Create(50));

        var eventStore = Fixture.Services.GetRequiredService<IEventStore>();
        await WriteStore.UpdateAsync(product);

        var events = await eventStore.GetByAggregateIdAsync(product.Id);
        events.Should().Contain(e => e.EventType.Contains("ProductUpdatedEvent"));
    }

    [Fact]
    public async Task Deactivate_Should_PersistDeactivation()
    {
        var product = CreateSampleProduct();
        await WriteStore.InsertAsync(product);
        product.ClearDomainEvents();

        product.Deactivate();
        await WriteStore.UpdateAsync(product);

        var result = await ReadStore.GetByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_Should_PersistDeactivatedEvent()
    {
        var product = CreateSampleProduct();
        await WriteStore.InsertAsync(product);
        product.ClearDomainEvents();

        product.Deactivate();

        var eventStore = Fixture.Services.GetRequiredService<IEventStore>();
        await WriteStore.UpdateAsync(product);

        var events = await eventStore.GetByAggregateIdAsync(product.Id);
        events.Should().Contain(e => e.EventType.Contains("ProductDeactivatedEvent"));
    }
}
