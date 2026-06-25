using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Products.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Products.Domain;

public class BarcodeTests
{
    [Theory]
    [InlineData("12345678")]
    [InlineData("ABC-123-XYZ")]
    [InlineData("  12345678  ")]
    public void Create_WithValidValue_Should_ReturnBarcode(string input)
    {
        var barcode = Barcode.Create(input);
        barcode.Value.Should().Be(input.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyValue_Should_Throw(string? input)
    {
        Action act = () => Barcode.Create(input!);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Barcode.Empty");
    }

    [Fact]
    public void Create_WithTooLongValue_Should_Throw()
    {
        var longValue = new string('x', 65);
        Action act = () => Barcode.Create(longValue);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Barcode.TooLong");
    }

    [Fact]
    public void Equals_WithSameValue_Should_BeTrue()
    {
        var a = Barcode.Create("12345678");
        var b = Barcode.Create("12345678");
        a.Should().Be(b);
    }

    [Fact]
    public void Equals_WithDifferentValue_Should_BeFalse()
    {
        var a = Barcode.Create("12345678");
        var b = Barcode.Create("87654321");
        a.Should().NotBe(b);
    }

    [Fact]
    public void Equals_WithDifferentCasing_Should_BeFalse()
    {
        var a = Barcode.Create("ABC123");
        var b = Barcode.Create("abc123");
        a.Should().NotBe(b);
    }
}
