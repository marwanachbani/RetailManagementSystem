using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Identity.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Identity.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user.name+tag@example.co.uk")]
    [InlineData("a@b.co")]
    public void Create_WithValidEmail_Should_ReturnNormalizedEmail(string input)
    {
        var email = Email.Create(input);
        email.Value.Should().Be(input.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_Should_Throw(string? input)
    {
        Action act = () => Email.Create(input!);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Email.Empty");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    [InlineData("user@@example.com")]
    public void Create_WithInvalidFormat_Should_Throw(string input)
    {
        Action act = () => Email.Create(input);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Email.InvalidFormat");
    }

    [Fact]
    public void Create_WithTooLongEmail_Should_Throw()
    {
        var local = new string('a', 250);
        var input = $"{local}@example.com";

        Action act = () => Email.Create(input);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Email.TooLong");
    }

    [Fact]
    public void Equals_WithSameValue_Should_BeTrue()
    {
        var a = Email.Create("test@example.com");
        var b = Email.Create("TEST@EXAMPLE.COM");
        a.Should().Be(b);
    }

    [Fact]
    public void Equals_WithDifferentValue_Should_BeFalse()
    {
        var a = Email.Create("a@example.com");
        var b = Email.Create("b@example.com");
        a.Should().NotBe(b);
    }
}
