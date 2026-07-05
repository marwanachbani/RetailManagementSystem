using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseOrderItem : Entity<Guid>
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }
    public int ReceivedQuantity { get; private set; }

    private PurchaseOrderItem() { }

    public static PurchaseOrderItem Create(Guid id, Guid purchaseOrderId, Guid productId, string productName, int quantity, decimal unitCost)
    {
        if (quantity <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrderItem.InvalidQuantity", "Quantity must be greater than zero.");
        if (unitCost <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrderItem.InvalidUnitCost", "Unit cost must be greater than zero.");

        return new PurchaseOrderItem
        {
            Id = id,
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = quantity * unitCost,
            ReceivedQuantity = 0
        };
    }

    public static PurchaseOrderItem Rehydrate(Guid id, Guid purchaseOrderId, Guid productId, string productName, int quantity, decimal unitCost, int receivedQuantity)
    {
        return new PurchaseOrderItem
        {
            Id = id,
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = quantity * unitCost,
            ReceivedQuantity = receivedQuantity
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrderItem.InvalidQuantity", "Quantity must be greater than zero.");

        Quantity = quantity;
        TotalCost = Quantity * UnitCost;
    }

    public void UpdateUnitCost(decimal unitCost)
    {
        if (unitCost <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrderItem.InvalidUnitCost", "Unit cost must be greater than zero.");

        UnitCost = unitCost;
        TotalCost = Quantity * UnitCost;
    }

    public void IncreaseReceivedQuantity(int amount)
    {
        if (amount <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrderItem.InvalidReceiptQuantity", "Received amount must be greater than zero.");
        if (ReceivedQuantity + amount > Quantity)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrderItem.OverReceipt", "Received quantity cannot exceed ordered quantity.");

        ReceivedQuantity += amount;
    }
}
