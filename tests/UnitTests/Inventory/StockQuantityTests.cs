using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Inventory.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Inventory;

public class StockQuantityTests
{
    [Fact]
    public void Create_WithPositiveValue_Should_Succeed()
    {
        var quantity = StockQuantity.Create(100);
        quantity.Value.Should().Be(100);
    }

    [Fact]
    public void Create_WithZero_Should_Succeed()
    {
        var quantity = StockQuantity.Create(0);
        quantity.Value.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegativeValue_Should_Throw()
    {
        Action act = () => StockQuantity.Create(-1);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "StockQuantity.Negative");
    }

    [Fact]
    public void Add_Should_IncreaseValue()
    {
        var quantity = StockQuantity.Create(10);
        var result = quantity.Add(5);
        result.Value.Should().Be(15);
    }

    [Fact]
    public void Add_WithNegativeAmount_Should_Throw()
    {
        var quantity = StockQuantity.Create(10);
        Action act = () => quantity.Add(-5);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "StockQuantity.InvalidAdd");
    }

    [Fact]
    public void Subtract_Should_DecreaseValue()
    {
        var quantity = StockQuantity.Create(10);
        var result = quantity.Subtract(5);
        result.Value.Should().Be(5);
    }

    [Fact]
    public void Subtract_ToZero_Should_Succeed()
    {
        var quantity = StockQuantity.Create(10);
        var result = quantity.Subtract(10);
        result.Value.Should().Be(0);
    }

    [Fact]
    public void Subtract_BelowZero_Should_Throw()
    {
        var quantity = StockQuantity.Create(5);
        Action act = () => quantity.Subtract(10);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "StockQuantity.InsufficientStock");
    }

    [Fact]
    public void Subtract_WithNegativeAmount_Should_Throw()
    {
        var quantity = StockQuantity.Create(10);
        Action act = () => quantity.Subtract(-5);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "StockQuantity.InvalidSubtract");
    }

    [Fact]
    public void IsBelowThreshold_WhenBelow_Should_ReturnTrue()
    {
        var quantity = StockQuantity.Create(5);
        quantity.IsBelowThreshold(10).Should().BeTrue();
    }

    [Fact]
    public void IsBelowThreshold_WhenEqual_Should_ReturnFalse()
    {
        var quantity = StockQuantity.Create(10);
        quantity.IsBelowThreshold(10).Should().BeFalse();
    }

    [Fact]
    public void IsBelowThreshold_WhenAbove_Should_ReturnFalse()
    {
        var quantity = StockQuantity.Create(15);
        quantity.IsBelowThreshold(10).Should().BeFalse();
    }

    [Fact]
    public void Equality_SameValue_Should_BeEqual()
    {
        var a = StockQuantity.Create(10);
        var b = StockQuantity.Create(10);
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValue_Should_NotBeEqual()
    {
        var a = StockQuantity.Create(10);
        var b = StockQuantity.Create(20);
        a.Should().NotBe(b);
    }
}
