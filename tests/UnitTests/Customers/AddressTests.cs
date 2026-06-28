using FluentAssertions;
using RMS.Modules.Customers.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Customers;

public class AddressTests
{
    [Fact]
    public void Create_WithValidData_Should_Succeed()
    {
        var address = Address.Create("123 Main St", "New York", "10001", "USA");

        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("New York");
        address.PostalCode.Should().Be("10001");
        address.Country.Should().Be("USA");
    }

    [Fact]
    public void Create_WithMinimalData_Should_Succeed()
    {
        var address = Address.Create("Main Street", "Boston");

        address.Street.Should().Be("Main Street");
        address.City.Should().Be("Boston");
        address.PostalCode.Should().BeNull();
        address.Country.Should().BeNull();
    }

    [Fact]
    public void Create_Should_TrimWhitespace()
    {
        var address = Address.Create("  123 Main St  ", "  New York  ", "  10001  ", "  USA  ");

        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("New York");
        address.PostalCode.Should().Be("10001");
        address.Country.Should().Be("USA");
    }

    [Fact]
    public void Create_WithNullOptionalFields_Should_Succeed()
    {
        var address = Address.Create("123 Main St", "New York", null, null);

        address.PostalCode.Should().BeNull();
        address.Country.Should().BeNull();
    }

    [Fact]
    public void Equals_SameValue_Should_BeEqual()
    {
        var address1 = Address.Create("123 Main St", "New York", "10001", "USA");
        var address2 = Address.Create("123 Main St", "New York", "10001", "USA");

        address1.Should().Be(address2);
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValue_Should_NotBeEqual()
    {
        var address1 = Address.Create("123 Main St", "New York", "10001", "USA");
        var address2 = Address.Create("456 Oak Ave", "New York", "10001", "USA");

        address1.Should().NotBe(address2);
    }

    [Fact]
    public void Equals_DifferentCity_Should_NotBeEqual()
    {
        var address1 = Address.Create("123 Main St", "New York", "10001", "USA");
        var address2 = Address.Create("123 Main St", "Boston", "10001", "USA");

        address1.Should().NotBe(address2);
    }
}
