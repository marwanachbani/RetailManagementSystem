using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Products.Domain.Entities;

public sealed class Category : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Category() { }

    public static Category Create(Guid id, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Category.NameEmpty", "Category name is required.");

        if (name.Trim().Length > 100)
            throw new BusinessRuleValidationException("Category.NameTooLong", "Category name must not exceed 100 characters.");

        return new Category
        {
            Id = id,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };
    }
}
