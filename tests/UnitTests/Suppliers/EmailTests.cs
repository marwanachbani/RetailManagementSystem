using FluentAssertions;
using RMS.Modules.Suppliers.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Suppliers;

public class EmailTests
{
    [Fact]
    public void Create_With_Valid_Email_Should_Succeed()
    {
        var email = Email.Create("test@example.com");
        email.Should().NotBeNull();
        email!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_With_Null_Should_Return_Null()
    {
        var email = Email.Create(null);
        email.Should().BeNull();
    }

    [Fact]
    public void Create_With_Empty_Should_Return_Null()
    {
        var email = Email.Create("");
        email.Should().BeNull();
    }

    [Fact]
    public void Create_Should_Normalize_To_Lowercase()
    {
        var email = Email.Create("Test@Example.COM");
        email!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_With_Invalid_Format_Should_Throw()
    {
        var act = () => Email.Create("not-an-email");
        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void Create_With_Too_Long_Value_Should_Throw()
    {
        var act = () => Email.Create(new string('a', 250) + "@example.com");
        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void Two_Emails_With_Same_Value_Should_Be_Equal()
    {
        var e1 = Email.Create("test@example.com");
        var e2 = Email.Create("test@example.com");
        e1.Should().Be(e2);
        e1!.GetHashCode().Should().Be(e2!.GetHashCode());
    }

    [Fact]
    public void Two_Emails_With_Different_Values_Should_Not_Be_Equal()
    {
        var e1 = Email.Create("a@example.com");
        var e2 = Email.Create("b@example.com");
        e1.Should().NotBe(e2);
    }
}
