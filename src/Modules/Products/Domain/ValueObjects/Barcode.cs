using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Products.Domain.ValueObjects;

public sealed class Barcode : ValueObject
{
    public string Value { get; }

    private Barcode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleValidationException("Barcode.Empty", "Barcode value is required.");
        
        Value = value.Trim();

        if (Value.Length > 64)
            throw new BusinessRuleValidationException("Barcode.TooLong", "Barcode must not exceed 64 characters.");
    }

    public static Barcode Create(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
