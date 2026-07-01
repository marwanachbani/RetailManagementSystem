using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Suppliers.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleValidationException("PhoneNumber.Empty", "Phone number is required.");

        var normalized = value.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (normalized.Length < 7 || normalized.Length > 15)
            throw new BusinessRuleValidationException("PhoneNumber.InvalidLength", "Phone number must be between 7 and 15 characters.");

        if (!normalized.All(c => char.IsDigit(c) || c == '+' || c == ' '))
            throw new BusinessRuleValidationException("PhoneNumber.InvalidFormat", "Phone number contains invalid characters.");

        return new PhoneNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
