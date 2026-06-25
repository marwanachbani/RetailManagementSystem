using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Inventory.Domain.ValueObjects;

public sealed class StockQuantity : ValueObject
{
    public int Value { get; }

    private StockQuantity(int value)
    {
        if (value < 0)
            throw new BusinessRuleValidationException("StockQuantity.Negative", "Stock quantity cannot be negative.");

        Value = value;
    }

    public static StockQuantity Create(int value) => new(value);

    public static StockQuantity Zero => new(0);

    public StockQuantity Add(int amount)
    {
        if (amount < 0)
            throw new BusinessRuleValidationException("StockQuantity.InvalidAdd", "Cannot add a negative amount. Use Decrease instead.");
        return new StockQuantity(Value + amount);
    }

    public StockQuantity Subtract(int amount)
    {
        if (amount < 0)
            throw new BusinessRuleValidationException("StockQuantity.InvalidSubtract", "Cannot subtract a negative amount. Use Increase instead.");
        if (amount > Value)
            throw new BusinessRuleValidationException("StockQuantity.InsufficientStock", "Insufficient stock for the requested operation.");
        return new StockQuantity(Value - amount);
    }

    public bool IsBelowThreshold(int threshold) => Value < threshold;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
