using System.Text.RegularExpressions;
using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Suppliers.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email? Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim().ToLowerInvariant();

        if (trimmed.Length > 255)
            throw new BusinessRuleValidationException("Email.TooLong", "Email must not exceed 255 characters.");

        if (!EmailRegex.IsMatch(trimmed))
            throw new BusinessRuleValidationException("Email.InvalidFormat", "Email format is invalid.");

        return new Email(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
