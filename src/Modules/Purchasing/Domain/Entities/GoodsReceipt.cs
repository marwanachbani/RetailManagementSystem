using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Purchasing.Domain.Entities;

public sealed class GoodsReceipt : Entity<Guid>
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int QuantityReceived { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public string? BatchNumber { get; private set; }
    public DateTime? ExpiryDate { get; private set; }

    private GoodsReceipt() { }

    public static GoodsReceipt Create(Guid id, Guid purchaseOrderId, Guid productId, int quantityReceived, string? batchNumber = null, DateTime? expiryDate = null)
    {
        if (quantityReceived <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("GoodsReceipt.InvalidQuantity", "Received quantity must be greater than zero.");

        return new GoodsReceipt
        {
            Id = id,
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            QuantityReceived = quantityReceived,
            ReceivedAt = DateTime.UtcNow,
            BatchNumber = batchNumber,
            ExpiryDate = expiryDate
        };
    }

    public static GoodsReceipt Rehydrate(Guid id, Guid purchaseOrderId, Guid productId, int quantityReceived, DateTime receivedAt, string? batchNumber, DateTime? expiryDate)
    {
        return new GoodsReceipt
        {
            Id = id,
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            QuantityReceived = quantityReceived,
            ReceivedAt = receivedAt,
            BatchNumber = batchNumber,
            ExpiryDate = expiryDate
        };
    }
}
