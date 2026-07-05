using FluentAssertions;
using RMS.Modules.Suppliers.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Suppliers;

public class PhoneNumberTests
{
    [Fact]
    public void Create_With_Valid_Number_Should_Succeed()
    {
        var phone = PhoneNumber.Create("+1234567890");
        phone.Value.Should().Be("+1234567890");
    }

    [Fact]
    public void Create_Should_Normalize_Number()
    {
        var phone = PhoneNumber.Create("+1 (234) 567-890");
        phone.Value.Should().Be("+1234567890");
    }

    [Fact]
    public void Create_With_Null_Should_Throw()
    {
        var act = () => PhoneNumber.Create(null);
        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void Create_With_Empty_Should_Throw()
    {
        var act = () => PhoneNumber.Create("");
        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void Create_With_Too_Short_Number_Should_Throw()
    {
        var act = () => PhoneNumber.Create("+123");
        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void Create_With_Too_Long_Number_Should_Throw()
    {
        var act = () => PhoneNumber.Create("+1234567890123456");
        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void Two_PhoneNumbers_With_Same_Normalized_Value_Should_Be_Equal()
    {
        var p1 = PhoneNumber.Create("+1234567890");
        var p2 = PhoneNumber.Create("+1 234 567 890");
        p1.Should().Be(p2);
        p1.GetHashCode().Should().Be(p2.GetHashCode());
    }

    [Fact]
    public void Two_PhoneNumbers_With_Different_Values_Should_Not_Be_Equal()
    {
        var p1 = PhoneNumber.Create("+1234567890");
        var p2 = PhoneNumber.Create("+9876543210");
        p1.Should().NotBe(p2);
    }
}
