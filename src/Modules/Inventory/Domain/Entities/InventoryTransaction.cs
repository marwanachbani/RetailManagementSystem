using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Inventory.Domain.Entities;

public sealed class InventoryTransaction : Entity<Guid>
{
    public Guid InventoryItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public int QuantityBefore { get; private set; }
    public int QuantityAfter { get; private set; }
    public int ChangeAmount { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public DateTime Timestamp { get; private set; }

    private InventoryTransaction() { }

    public static InventoryTransaction Create(
        Guid id,
        Guid inventoryItemId,
        Guid productId,
        int quantityBefore,
        int quantityAfter,
        int changeAmount,
        string reason,
        Guid? userId = null)
    {
        return new InventoryTransaction
        {
            Id = id,
            InventoryItemId = inventoryItemId,
            ProductId = productId,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
            ChangeAmount = changeAmount,
            Reason = reason.Trim(),
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };
    }
}
