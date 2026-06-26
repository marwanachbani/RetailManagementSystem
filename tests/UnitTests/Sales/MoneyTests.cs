using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Sales.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Sales;

public class MoneyTests
{
    [Fact]
    public void Create_WithPositiveValue_Should_Succeed()
    {
        var money = Money.Create(100.50m);
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("MAD");
    }

    [Fact]
    public void Create_WithNegativeValue_Should_Throw()
    {
        Action act = () => Money.Create(-1);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Money.AmountLessThanZero");
    }

    [Fact]
    public void Add_SameCurrency_Should_Succeed()
    {
        var a = Money.Create(50);
        var b = Money.Create(30);
        var result = a.Add(b);
        result.Amount.Should().Be(80);
    }

    [Fact]
    public void Add_DifferentCurrency_Should_Throw()
    {
        var a = Money.Create(50, "MAD");
        var b = Money.Create(30, "USD");
        Action act = () => a.Add(b);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Money.CurrencyMismatch");
    }

    [Fact]
    public void Subtract_Should_DecreaseAmount()
    {
        var a = Money.Create(50);
        var b = Money.Create(20);
        var result = a.Subtract(b);
        result.Amount.Should().Be(30);
    }

    [Fact]
    public void Subtract_BelowZero_Should_Throw()
    {
        var a = Money.Create(10);
        var b = Money.Create(20);
        Action act = () => a.Subtract(b);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Money.Insufficient");
    }

    [Fact]
    public void Multiply_Should_ScaleAmount()
    {
        var money = Money.Create(100);
        var result = money.Multiply(1.5m);
        result.Amount.Should().Be(150);
    }

    [Fact]
    public void ApplyDiscount_Should_ReduceAmount()
    {
        var money = Money.Create(100);
        var result = money.ApplyDiscount(10);
        result.Amount.Should().Be(90);
    }

    [Fact]
    public void ApplyDiscount_100Percent_Should_BeZero()
    {
        var money = Money.Create(100);
        var result = money.ApplyDiscount(100);
        result.Amount.Should().Be(0);
    }

    [Fact]
    public void ApplyDiscount_Over100Percent_Should_Throw()
    {
        var money = Money.Create(100);
        Action act = () => money.ApplyDiscount(101);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Money.InvalidDiscount");
    }

    [Fact]
    public void AddTax_Should_IncreaseAmount()
    {
        var money = Money.Create(100);
        var result = money.AddTax(20);
        result.Amount.Should().Be(120);
    }

    [Fact]
    public void Equality_SameValue_Should_BeEqual()
    {
        var a = Money.Create(100);
        var b = Money.Create(100);
        a.Should().Be(b);
    }
}
