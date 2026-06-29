using FluentAssertions;
using RMS.Modules.Customers.Domain.ValueObjects;
using RMS.BuildingBlocks.Exceptions;
using Xunit;

namespace RMS.UnitTests.Customers;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.org")]
    [InlineData("a@b.co")]
    public void Create_WithValidEmail_Should_Succeed(string email)
    {
        var result = Email.Create(email);

        result.Should().NotBeNull();
        result.Value.Should().Be(email.Trim().ToLowerInvariant());
    }

    [Fact]
    public void Create_WithEmptyEmail_Should_Throw()
    {
        var act = () => Email.Create("");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Email.Empty");
    }

    [Fact]
    public void Create_WithNullEmail_Should_Throw()
    {
        var act = () => Email.Create(null!);

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Email.Empty");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    [InlineData("user name@example.com")]
    public void Create_WithInvalidFormat_Should_Throw(string email)
    {
        var act = () => Email.Create(email);

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Email.InvalidFormat");
    }

    [Fact]
    public void Create_WithTooLongEmail_Should_Throw()
    {
        var local = new string('a', 250);
        var email = $"{local}@example.com";

        var act = () => Email.Create(email);

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Email.TooLong");
    }

    [Fact]
    public void Create_Should_NormalizeToLowerCase()
    {
        var result = Email.Create("Test@Example.COM");

        result.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Equals_SameValue_Should_BeEqual()
    {
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("Test@Example.COM");

        email1.Should().Be(email2);
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValue_Should_NotBeEqual()
    {
        var email1 = Email.Create("a@example.com");
        var email2 = Email.Create("b@example.com");

        email1.Should().NotBe(email2);
    }
}
