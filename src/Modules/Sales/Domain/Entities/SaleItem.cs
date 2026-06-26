using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Sales.Domain.Entities;

public sealed class SaleItem : Entity<Guid>
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    private SaleItem() { }

    public static SaleItem Create(
        Guid id,
        Guid saleId,
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (quantity <= 0)
            throw new BusinessRuleValidationException("SaleItem.InvalidQuantity", "Quantity must be greater than zero.");
        if (unitPrice < 0)
            throw new BusinessRuleValidationException("SaleItem.NegativePrice", "Unit price cannot be negative.");
        if (string.IsNullOrWhiteSpace(productName))
            throw new BusinessRuleValidationException("SaleItem.EmptyProductName", "Product name is required.");

        return new SaleItem
        {
            Id = id,
            SaleId = saleId,
            ProductId = productId,
            ProductName = productName.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleValidationException("SaleItem.InvalidQuantity", "Quantity must be greater than zero.");
        Quantity = quantity;
    }
}
