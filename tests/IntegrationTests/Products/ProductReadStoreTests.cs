using FluentAssertions;
using RMS.Modules.Products.Application.Contracts;
using RMS.Modules.Products.Domain.Entities;
using RMS.Modules.Products.Domain.ValueObjects;
using RMS.Modules.Products.Infrastructure.Migrations;
using Xunit;

namespace RMS.IntegrationTests.Products;

public class ProductReadStoreTests : ProductsIntegrationTestBase, IClassFixture<TestDatabaseFixture>
{
    public ProductReadStoreTests(TestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_Should_ReturnProduct()
    {
        var product = CreateSampleProduct();
        await WriteStore.InsertAsync(product);

        var result = await ReadStore.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Sample Product");
        result.ProductCode.Should().Be(product.ProductCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotExists_Should_ReturnNull()
    {
        var result = await ReadStore.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBarcodeAsync_WhenProductExists_Should_ReturnProduct()
    {
        var product = CreateSampleProduct(barcode: "UNIQUE-BAR-123");
        await WriteStore.InsertAsync(product);

        var result = await ReadStore.GetByBarcodeAsync("UNIQUE-BAR-123");

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetByBarcodeAsync_WhenProductNotExists_Should_ReturnNull()
    {
        var result = await ReadStore.GetByBarcodeAsync("NONEXISTENT");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_WithMatchingTerm_Should_ReturnProducts()
    {
        var product = CreateSampleProduct(name: "Special Widget");
        await WriteStore.InsertAsync(product);

        var results = await ReadStore.SearchAsync("Special", false);

        results.Should().ContainSingle();
        results[0].Name.Should().Be("Special Widget");
    }

    [Fact]
    public async Task SearchAsync_WithNoMatch_Should_ReturnEmptyList()
    {
        var results = await ReadStore.SearchAsync("XYZ-NO-MATCH", false);
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithInactive_Should_ExcludeInactive_WhenIncludeInactiveIsFalse()
    {
        var product = CreateSampleProduct();
        await WriteStore.InsertAsync(product);
        product.ClearDomainEvents();

        product.Deactivate();
        await WriteStore.UpdateAsync(product);

        var results = await ReadStore.SearchAsync("Sample", false);
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithInactive_Should_IncludeInactive_WhenIncludeInactiveIsTrue()
    {
        var product = CreateSampleProduct();
        await WriteStore.InsertAsync(product);
        product.ClearDomainEvents();

        product.Deactivate();
        await WriteStore.UpdateAsync(product);

        var results = await ReadStore.SearchAsync("Sample", true);
        results.Should().ContainSingle();
    }

    [Fact]
    public async Task GetPagedAsync_Should_ReturnPagedResult()
    {
        for (int i = 0; i < 5; i++)
        {
            var product = CreateSampleProduct(
                id: Guid.NewGuid(),
                barcode: $"BAR-{i}");
            await WriteStore.InsertAsync(product);
        }

        var page = await ReadStore.GetPagedAsync(1, 2, null, false);

        page.Items.Should().HaveCount(2);
        page.PageNumber.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.TotalCount.Should().Be(5);
        page.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetCategoriesAsync_Should_ReturnSeedCategories()
    {
        var categories = await ReadStore.GetCategoriesAsync();

        categories.Should().HaveCount(3);
        categories.Should().Contain(c => c.Name == "Electronics");
        categories.Should().Contain(c => c.Name == "Clothing");
        categories.Should().Contain(c => c.Name == "Groceries");
    }

    private static Product CreateSampleProduct(Guid? id = null, string? name = null, string? barcode = null)
    {
        return Product.Create(
            id ?? Guid.NewGuid(),
            name ?? "Sample Product",
            "Sample Description",
            Barcode.Create(barcode ?? $"BAR-{Guid.NewGuid():N}"),
            CreateProductsTablesMigration.ElectronicsCategoryId,
            Money.Create(100),
            Money.Create(50));
    }
}
