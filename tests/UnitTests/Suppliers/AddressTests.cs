using FluentAssertions;
using RMS.Modules.Suppliers.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Suppliers;

public class AddressTests
{
    [Fact]
    public void Create_With_Valid_Data_Should_Succeed()
    {
        var address = Address.Create("123 Main St", "Springfield", "12345", "USA");
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Springfield");
        address.PostalCode.Should().Be("12345");
        address.Country.Should().Be("USA");
    }

    [Fact]
    public void Create_With_Null_Optionals_Should_Succeed()
    {
        var address = Address.Create("123 Main St", "Springfield");
        address.PostalCode.Should().BeNull();
        address.Country.Should().BeNull();
    }

    [Fact]
    public void Create_Should_Trim_Values()
    {
        var address = Address.Create(" 123 Main St ", " Springfield ", " 12345 ", " USA ");
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Springfield");
    }

    [Fact]
    public void Two_Addresses_With_Same_Values_Should_Be_Equal()
    {
        var a1 = Address.Create("123 Main St", "Springfield", "12345", "USA");
        var a2 = Address.Create("123 Main St", "Springfield", "12345", "USA");
        a1.Should().Be(a2);
        a1.GetHashCode().Should().Be(a2.GetHashCode());
    }

    [Fact]
    public void Two_Addresses_With_Different_Values_Should_Not_Be_Equal()
    {
        var a1 = Address.Create("123 Main St", "Springfield", "12345", "USA");
        var a2 = Address.Create("456 Oak St", "Shelbyville", "54321", "USA");
        a1.Should().NotBe(a2);
    }
}
