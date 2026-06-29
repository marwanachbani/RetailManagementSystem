using FluentAssertions;
using RMS.Modules.Customers.Domain.ValueObjects;
using RMS.BuildingBlocks.Exceptions;
using Xunit;

namespace RMS.UnitTests.Customers;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+1 234 567 890")]
    [InlineData("+1-234-567-890")]
    [InlineData("1234567")]
    [InlineData("123456789012345")]
    public void Create_WithValidPhoneNumber_Should_Succeed(string phoneNumber)
    {
        var result = PhoneNumber.Create(phoneNumber);

        result.Should().NotBeNull();
        result.Value.Should().Be(phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", ""));
    }

    [Fact]
    public void Create_WithEmptyPhoneNumber_Should_Throw()
    {
        var act = () => PhoneNumber.Create("");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "PhoneNumber.Empty");
    }

    [Fact]
    public void Create_WithNullPhoneNumber_Should_Throw()
    {
        var act = () => PhoneNumber.Create(null!);

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "PhoneNumber.Empty");
    }

    [Fact]
    public void Create_WithTooShortPhoneNumber_Should_Throw()
    {
        var act = () => PhoneNumber.Create("123456");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "PhoneNumber.InvalidLength");
    }

    [Fact]
    public void Create_WithTooLongPhoneNumber_Should_Throw()
    {
        var act = () => PhoneNumber.Create("1234567890123456");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "PhoneNumber.InvalidLength");
    }

    [Theory]
    [InlineData("abc1234567")]
    [InlineData("12-34-56-abc")]
    [InlineData("+123 456 @789")]
    public void Create_WithInvalidCharacters_Should_Throw(string phoneNumber)
    {
        var act = () => PhoneNumber.Create(phoneNumber);

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "PhoneNumber.InvalidFormat");
    }

    [Fact]
    public void Equals_SameValue_Should_BeEqual()
    {
        var phone1 = PhoneNumber.Create("+1234567890");
        var phone2 = PhoneNumber.Create("+1 234 567 890");

        phone1.Should().Be(phone2);
        phone1.GetHashCode().Should().Be(phone2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValue_Should_NotBeEqual()
    {
        var phone1 = PhoneNumber.Create("+1234567890");
        var phone2 = PhoneNumber.Create("+9876543210");

        phone1.Should().NotBe(phone2);
    }
}
