using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Products.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Products.Domain;

public class MoneyTests
{
    [Theory]
    [InlineData(0, "USD")]
    [InlineData(100.50, "MAD")]
    [InlineData(999999.99, "EUR")]
    public void Create_WithValidData_Should_ReturnMoney(decimal amount, string currency)
    {
        var money = Money.Create(amount, currency);
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency.ToUpperInvariant());
    }

    [Fact]
    public void Create_WithDefaultCurrency_Should_UseUsd()
    {
        var money = Money.Create(100);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_Should_Throw()
    {
        Action act = () => Money.Create(-1);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Money.AmountLessThanZero");
    }

    [Fact]
    public void Create_WithNullCurrency_Should_UseUsd()
    {
        var money = Money.Create(100, null!);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithEmptyCurrency_Should_UseUsd()
    {
        var money = Money.Create(100, "  ");
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Equals_WithSameValue_Should_BeTrue()
    {
        var a = Money.Create(100, "USD");
        var b = Money.Create(100, "USD");
        a.Should().Be(b);
    }

    [Fact]
    public void Equals_WithDifferentAmount_Should_BeFalse()
    {
        var a = Money.Create(100, "USD");
        var b = Money.Create(200, "USD");
        a.Should().NotBe(b);
    }

    [Fact]
    public void Equals_WithDifferentCurrency_Should_BeFalse()
    {
        var a = Money.Create(100, "USD");
        var b = Money.Create(100, "MAD");
        a.Should().NotBe(b);
    }
}
