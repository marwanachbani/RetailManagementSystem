using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Products.Domain.Entities;
using RMS.Modules.Products.Domain.Events;
using RMS.Modules.Products.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Products.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_Should_CreateProductAndRaiseEvent()
    {
        var product = CreateValidProduct();

        product.Name.Should().Be("Test Product");
        product.ProductCode.Should().StartWith("PRD-");
        product.IsActive.Should().BeTrue();
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyName_Should_Throw(string? name)
    {
        Action act = () => Product.Create(
            Guid.NewGuid(),
            name!,
            "Description",
            Barcode.Create("12345678"),
            Guid.NewGuid(),
            Money.Create(100),
            Money.Create(50));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Product.NameEmpty");
    }

    [Fact]
    public void Create_WithLongName_Should_Throw()
    {
        Action act = () => Product.Create(
            Guid.NewGuid(),
            new string('x', 151),
            "Description",
            Barcode.Create("12345678"),
            Guid.NewGuid(),
            Money.Create(100),
            Money.Create(50));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Product.NameTooLong");
    }

    [Fact]
    public void Create_WithEmptyCategoryId_Should_Throw()
    {
        Action act = () => Product.Create(
            Guid.NewGuid(),
            "Test",
            "Description",
            Barcode.Create("12345678"),
            Guid.Empty,
            Money.Create(100),
            Money.Create(50));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Product.CategoryEmpty");
    }

    [Fact]
    public void Create_WithSalePriceBelowCost_Should_Throw()
    {
        Action act = () => Product.Create(
            Guid.NewGuid(),
            "Test",
            "Description",
            Barcode.Create("12345678"),
            Guid.NewGuid(),
            Money.Create(50),
            Money.Create(100));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Product.SalePriceBelowCost");
    }

    [Fact]
    public void Update_WithValidData_Should_UpdateAndRaiseEvent()
    {
        var product = CreateValidProduct();
        product.ClearDomainEvents();

        var newCategoryId = Guid.NewGuid();
        product.Update(
            "Updated Name",
            "Updated Description",
            Barcode.Create("87654321"),
            newCategoryId,
            Money.Create(200),
            Money.Create(100));

        product.Name.Should().Be("Updated Name");
        product.Description.Should().Be("Updated Description");
        product.Barcode.Value.Should().Be("87654321");
        product.CategoryId.Should().Be(newCategoryId);
        product.SalePrice.Amount.Should().Be(200);
        product.CostPrice.Amount.Should().Be(100);
        product.UpdatedAt.Should().NotBeNull();
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductUpdatedEvent>();
    }

    [Fact]
    public void Update_WhenInactive_Should_Throw()
    {
        var product = CreateValidProduct();
        product.Deactivate();
        product.ClearDomainEvents();

        Action act = () => product.Update(
            "Updated",
            null,
            Barcode.Create("11111111"),
            Guid.NewGuid(),
            Money.Create(100),
            Money.Create(50));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Product.InactiveUpdate");
    }

    [Fact]
    public void Update_WithSalePriceBelowCost_Should_Throw()
    {
        var product = CreateValidProduct();
        product.ClearDomainEvents();

        Action act = () => product.Update(
            "Updated",
            null,
            Barcode.Create("11111111"),
            Guid.NewGuid(),
            Money.Create(50),
            Money.Create(100));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Product.SalePriceBelowCost");
    }

    [Fact]
    public void Deactivate_Should_SetIsActiveToFalseAndRaiseEvent()
    {
        var product = CreateValidProduct();
        product.ClearDomainEvents();

        product.Deactivate();

        product.IsActive.Should().BeFalse();
        product.UpdatedAt.Should().NotBeNull();
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_Should_NotRaiseEvent()
    {
        var product = CreateValidProduct();
        product.Deactivate();
        product.ClearDomainEvents();

        product.Deactivate();

        product.IsActive.Should().BeFalse();
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rehydrate_Should_ReturnProductWithoutRaisingEvents()
    {
        var id = Guid.NewGuid();
        var product = Product.Rehydrate(
            id,
            "PRD-ORIGINAL",
            "Test",
            "Description",
            Barcode.Create("12345678"),
            Guid.NewGuid(),
            Money.Create(100),
            Money.Create(50),
            true,
            DateTime.UtcNow.AddDays(-1),
            null);

        product.Id.Should().Be(id);
        product.ProductCode.Should().Be("PRD-ORIGINAL");
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ProductCode_Should_BeAutoGenerated()
    {
        var product = CreateValidProduct();
        product.ProductCode.Should().NotBeNullOrEmpty();
        product.ProductCode.Length.Should().BeGreaterThan(4);
    }

    private static Product CreateValidProduct()
    {
        return Product.Create(
            Guid.NewGuid(),
            "Test Product",
            "Test Description",
            Barcode.Create("12345678"),
            Guid.NewGuid(),
            Money.Create(100),
            Money.Create(50));
    }
}
