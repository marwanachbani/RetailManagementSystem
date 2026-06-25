using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Inventory.Domain.Events;
using RMS.Modules.Inventory.Domain.ValueObjects;

namespace RMS.Modules.Inventory.Domain.Entities;

public sealed class InventoryItem : AggregateRoot<Guid>
{
    public Guid ProductId { get; private set; }
    public StockQuantity CurrentQuantity { get; private set; } = StockQuantity.Zero;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int LowStockThreshold { get; private set; }

    private readonly List<InventoryTransaction> _transactions = new();
    public IReadOnlyCollection<InventoryTransaction> Transactions => _transactions.AsReadOnly();

    private InventoryItem() { }

    public static InventoryItem Rehydrate(
        Guid id,
        Guid productId,
        int currentQuantity,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt,
        int lowStockThreshold)
    {
        return new InventoryItem
        {
            Id = id,
            ProductId = productId,
            CurrentQuantity = StockQuantity.Create(currentQuantity),
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            LowStockThreshold = lowStockThreshold
        };
    }

    public static InventoryItem Create(Guid id, Guid productId, int initialQuantity = 0, int lowStockThreshold = 10)
    {
        ValidateProductId(productId);

        var item = new InventoryItem
        {
            Id = id,
            ProductId = productId,
            CurrentQuantity = StockQuantity.Create(initialQuantity),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LowStockThreshold = lowStockThreshold
        };

        item.Raise(new InventoryCreatedEvent(item.Id, item.ProductId, item.CurrentQuantity.Value));
        item.CheckLowStock();

        return item;
    }

    public void IncreaseStock(int amount, string reason, Guid? userId = null)
    {
        EnsureActive();
        EnsureReasonProvided(reason);
        if (amount <= 0)
            throw new BusinessRuleValidationException("Inventory.InvalidAmount", "Increase amount must be greater than zero.");

        var quantityBefore = CurrentQuantity.Value;
        CurrentQuantity = CurrentQuantity.Add(amount);
        UpdatedAt = DateTime.UtcNow;

        var transaction = InventoryTransaction.Create(
            Guid.NewGuid(), Id, ProductId, quantityBefore, CurrentQuantity.Value, amount, reason, userId);
        _transactions.Add(transaction);

        Raise(new StockIncreasedEvent(Id, ProductId, quantityBefore, CurrentQuantity.Value, amount, reason));
        CheckLowStock();
    }

    public void DecreaseStock(int amount, string reason, Guid? userId = null)
    {
        EnsureActive();
        EnsureReasonProvided(reason);
        if (amount <= 0)
            throw new BusinessRuleValidationException("Inventory.InvalidAmount", "Decrease amount must be greater than zero.");

        var quantityBefore = CurrentQuantity.Value;
        CurrentQuantity = CurrentQuantity.Subtract(amount);
        UpdatedAt = DateTime.UtcNow;

        var transaction = InventoryTransaction.Create(
            Guid.NewGuid(), Id, ProductId, quantityBefore, CurrentQuantity.Value, -amount, reason, userId);
        _transactions.Add(transaction);

        Raise(new StockDecreasedEvent(Id, ProductId, quantityBefore, CurrentQuantity.Value, amount, reason));
        CheckLowStock();
    }

    public void AdjustStock(int newQuantity, string reason, Guid? userId = null)
    {
        EnsureActive();
        EnsureReasonProvided(reason);

        var quantityBefore = CurrentQuantity.Value;
        var changeAmount = newQuantity - quantityBefore;
        CurrentQuantity = StockQuantity.Create(newQuantity);
        UpdatedAt = DateTime.UtcNow;

        var transaction = InventoryTransaction.Create(
            Guid.NewGuid(), Id, ProductId, quantityBefore, CurrentQuantity.Value, changeAmount, reason, userId);
        _transactions.Add(transaction);

        Raise(new StockAdjustedEvent(Id, ProductId, quantityBefore, CurrentQuantity.Value, changeAmount, reason));
        CheckLowStock();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLowStockThreshold(int threshold)
    {
        if (threshold < 0)
            throw new BusinessRuleValidationException("Inventory.InvalidThreshold", "Low stock threshold cannot be negative.");

        LowStockThreshold = threshold;
        UpdatedAt = DateTime.UtcNow;
        CheckLowStock();
    }

    private void CheckLowStock()
    {
        if (CurrentQuantity.IsBelowThreshold(LowStockThreshold))
        {
            Raise(new LowStockDetectedEvent(Id, ProductId, CurrentQuantity.Value, LowStockThreshold));
        }
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new BusinessRuleValidationException("Inventory.Inactive", "Inventory operations are not allowed for inactive items.");
    }

    private static void EnsureReasonProvided(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleValidationException("Inventory.ReasonRequired", "A reason is required for stock adjustments.");
    }

    private static void ValidateProductId(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new BusinessRuleValidationException("Inventory.ProductIdRequired", "ProductId is required.");
    }
}
