using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Identity.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Identity.Domain;

public class PasswordHashTests
{
    [Fact]
    public void Create_WithValidHash_Should_ReturnHash()
    {
        var hash = PasswordHash.Create("$2a$12$hashedvalue123");
        hash.Value.Should().Be("$2a$12$hashedvalue123");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyHash_Should_Throw(string? input)
    {
        Action act = () => PasswordHash.Create(input!);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "PasswordHash.Empty");
    }

    [Fact]
    public void Equals_WithSameValue_Should_BeTrue()
    {
        var a = PasswordHash.Create("hash_123");
        var b = PasswordHash.Create("hash_123");
        a.Should().Be(b);
    }

    [Fact]
    public void Equals_WithDifferentValue_Should_BeFalse()
    {
        var a = PasswordHash.Create("hash_123");
        var b = PasswordHash.Create("hash_456");
        a.Should().NotBe(b);
    }
}
