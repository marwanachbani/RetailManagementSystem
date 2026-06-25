using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Products.Domain.Entities;
using Xunit;

namespace RMS.UnitTests.Products.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_Should_ReturnCategory()
    {
        var category = Category.Create(Guid.NewGuid(), "Electronics", "Electronic devices and accessories");

        category.Name.Should().Be("Electronics");
        category.Description.Should().Be("Electronic devices and accessories");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyName_Should_Throw(string? name)
    {
        Action act = () => Category.Create(Guid.NewGuid(), name!);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Category.NameEmpty");
    }

    [Fact]
    public void Create_WithLongName_Should_Throw()
    {
        Action act = () => Category.Create(Guid.NewGuid(), new string('x', 101));
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Category.NameTooLong");
    }

    [Fact]
    public void Create_WithNullDescription_Should_SetNull()
    {
        var category = Category.Create(Guid.NewGuid(), "Test", null);
        category.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithWhitespaceDescription_Should_SetNull()
    {
        var category = Category.Create(Guid.NewGuid(), "Test", "   ");
        category.Description.Should().BeNull();
    }
}
