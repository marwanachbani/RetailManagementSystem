using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.CreateProduct;
using RMS.Modules.Products.Application.DeactivateProduct;
using RMS.Modules.Products.Application.GetProductById;
using RMS.Modules.Products.Application.GetProductsPaged;
using RMS.Modules.Products.Application.UpdateProduct;
using RMS.Modules.Products.Domain.Entities;
using RMS.Modules.Products.Domain.ValueObjects;
using RMS.Modules.Products.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Products;

public class CommandHandlerTests : ProductsIntegrationTestBase, IClassFixture<TestDatabaseFixture>
{
    private readonly IMediator _mediator;

    public CommandHandlerTests(TestDatabaseFixture fixture) : base(fixture)
    {
        _mediator = fixture.Services.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task CreateProduct_Should_ReturnProductId()
    {
        var command = new CreateProductCommand(
            "Integration Test Product",
            "Description",
            "INT-BAR-001",
            CreateProductsTablesMigration.ElectronicsCategoryId,
            100,
            50);

        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateBarcode_Should_Fail()
    {
        var product = Product.Create(
            Guid.NewGuid(),
            "First Product",
            null,
            Barcode.Create("DUP-BAR-001"),
            CreateProductsTablesMigration.ElectronicsCategoryId,
            Money.Create(100),
            Money.Create(50));
        await WriteStore.InsertAsync(product);

        var command = new CreateProductCommand(
            "Second Product",
            null,
            "DUP-BAR-001",
            CreateProductsTablesMigration.ElectronicsCategoryId,
            100,
            50);

        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Products.BarcodeAlreadyExists");
    }

    [Fact]
    public async Task UpdateProduct_Should_ModifyExistingProduct()
    {
        var product = Product.Create(
            Guid.NewGuid(),
            "Original Name",
            null,
            Barcode.Create("UPD-BAR-001"),
            CreateProductsTablesMigration.ElectronicsCategoryId,
            Money.Create(100),
            Money.Create(50));
        await WriteStore.InsertAsync(product);

        var command = new UpdateProductCommand(
            product.Id,
            "Updated Name",
            "Updated Description",
            "UPD-BAR-001",
            CreateProductsTablesMigration.ClothingCategoryId,
            200,
            100);

        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(product.Id);
        updated!.Name.Should().Be("Updated Name");
        updated.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistentId_Should_Fail()
    {
        var command = new UpdateProductCommand(
            Guid.NewGuid(),
            "Name",
            null,
            "BAR-999",
            CreateProductsTablesMigration.ElectronicsCategoryId,
            100,
            50);

        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Products.NotFound");
    }

    [Fact]
    public async Task DeactivateProduct_Should_SetIsActiveToFalse()
    {
        var product = Product.Create(
            Guid.NewGuid(),
            "To Deactivate",
            null,
            Barcode.Create("DEAC-BAR-001"),
            CreateProductsTablesMigration.ElectronicsCategoryId,
            Money.Create(100),
            Money.Create(50));
        await WriteStore.InsertAsync(product);

        var command = new DeactivateProductCommand(product.Id);
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(product.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateProduct_WithNonExistentId_Should_Fail()
    {
        var command = new DeactivateProductCommand(Guid.NewGuid());
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Products.NotFound");
    }

    [Fact]
    public async Task GetProductById_Should_ReturnProduct()
    {
        var product = Product.Create(
            Guid.NewGuid(),
            "Get Me",
            null,
            Barcode.Create("GET-BAR-001"),
            CreateProductsTablesMigration.ElectronicsCategoryId,
            Money.Create(100),
            Money.Create(50));
        await WriteStore.InsertAsync(product);

        var query = new GetProductByIdQuery(product.Id);
        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be("Get Me");
    }

    [Fact]
    public async Task GetProductById_WithNonExistentId_Should_Fail()
    {
        var query = new GetProductByIdQuery(Guid.NewGuid());
        var result = await _mediator.Send(query);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Products.NotFound");
    }

    [Fact]
    public async Task GetProductsPaged_Should_ReturnPagedResult()
    {
        for (int i = 0; i < 10; i++)
        {
            var product = Product.Create(
                Guid.NewGuid(),
                $"Paged Product {i}",
                null,
                Barcode.Create($"PAGE-BAR-{i}"),
                CreateProductsTablesMigration.ElectronicsCategoryId,
                Money.Create(100 + i),
                Money.Create(50 + i));
            await WriteStore.InsertAsync(product);
        }

        var query = new GetProductsPagedQuery(1, 5, null, false);
        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().BeGreaterThanOrEqualTo(10);
    }
}
