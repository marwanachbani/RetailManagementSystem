using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Sales.Domain.Entities;
using Xunit;

namespace RMS.UnitTests.Sales;

public class SaleItemTests
{
    [Fact]
    public void Create_WithValidValues_Should_Succeed()
    {
        var item = SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Widget", 5, 10.00m);

        item.ProductName.Should().Be("Widget");
        item.Quantity.Should().Be(5);
        item.UnitPrice.Should().Be(10.00m);
        item.TotalPrice.Should().Be(50.00m);
    }

    [Fact]
    public void Create_WithZeroQuantity_Should_Throw()
    {
        Action act = () => SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Widget", 0, 10.00m);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "SaleItem.InvalidQuantity");
    }

    [Fact]
    public void Create_WithNegativePrice_Should_Throw()
    {
        Action act = () => SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Widget", 1, -10.00m);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "SaleItem.NegativePrice");
    }

    [Fact]
    public void Create_WithEmptyProductName_Should_Throw()
    {
        Action act = () => SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "", 1, 10.00m);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "SaleItem.EmptyProductName");
    }

    [Fact]
    public void UpdateQuantity_Should_ChangeQuantity()
    {
        var item = SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Widget", 2, 10.00m);
        item.UpdateQuantity(5);
        item.Quantity.Should().Be(5);
        item.TotalPrice.Should().Be(50.00m);
    }

    [Fact]
    public void UpdateQuantity_WithZero_Should_Throw()
    {
        var item = SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Widget", 2, 10.00m);
        Action act = () => item.UpdateQuantity(0);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "SaleItem.InvalidQuantity");
    }
}
