using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Products.Domain.Events;
using RMS.Modules.Products.Domain.ValueObjects;

namespace RMS.Modules.Products.Domain.Entities;

public sealed class Product : AggregateRoot<Guid>
{
    public string ProductCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Barcode Barcode { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public Money SalePrice { get; private set; } = null!;
    public Money CostPrice { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Product() { }

    public static Product Create(
        Guid id,
        string name,
        string? description,
        Barcode barcode,
        Guid categoryId,
        Money salePrice,
        Money costPrice)
    {
        ValidateName(name);
        ValidateCategory(categoryId);
        ValidatePrices(salePrice, costPrice);

        var product = new Product
        {
            Id = id,
            ProductCode = $"PRD-{id.ToString("N")[..8].ToUpperInvariant()}",
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Barcode = barcode,
            CategoryId = categoryId,
            SalePrice = salePrice,
            CostPrice = costPrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        product.Raise(new ProductCreatedEvent(
            product.Id,
            product.ProductCode,
            product.Name,
            product.CategoryId,
            product.SalePrice.Amount,
            product.CostPrice.Amount));

        return product;
    }

    public void Update(
        string name,
        string? description,
        Barcode barcode,
        Guid categoryId,
        Money salePrice,
        Money costPrice)
    {
        if (!IsActive)
            throw new BusinessRuleValidationException("Product.InactiveUpdate", "Inactive products cannot be updated.");

        ValidateName(name);
        ValidateCategory(categoryId);
        ValidatePrices(salePrice, costPrice);

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Barcode = barcode;
        CategoryId = categoryId;
        SalePrice = salePrice;
        CostPrice = costPrice;
        UpdatedAt = DateTime.UtcNow;

        Raise(new ProductUpdatedEvent(Id, ProductCode, Name, CategoryId, SalePrice.Amount, CostPrice.Amount));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        Raise(new ProductDeactivatedEvent(Id, ProductCode));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Product.NameEmpty", "Product name is required.");

        if (name.Trim().Length > 150)
            throw new BusinessRuleValidationException("Product.NameTooLong", "Product name must not exceed 150 characters.");
    }

    private static void ValidateCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new BusinessRuleValidationException("Product.CategoryEmpty", "Category is required.");
    }

    private static void ValidatePrices(Money salePrice, Money costPrice)
    {
        if (salePrice.Amount < costPrice.Amount)
            throw new BusinessRuleValidationException("Product.SalePriceBelowCost", "Sale price must be greater than or equal to cost price.");
    }
}
