using System.Text.RegularExpressions;
using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Customers.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new BusinessRuleValidationException("PhoneNumber.Empty", "Phone number is required.");

        var trimmed = phoneNumber.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (trimmed.Length < 7 || trimmed.Length > 15)
            throw new BusinessRuleValidationException("PhoneNumber.InvalidLength", "Phone number must be between 7 and 15 digits.");

        if (!Regex.IsMatch(trimmed, @"^\+?[0-9]+$"))
            throw new BusinessRuleValidationException("PhoneNumber.InvalidFormat", "Phone number contains invalid characters.");

        return new PhoneNumber(trimmed);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
