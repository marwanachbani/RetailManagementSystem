using System.Text.RegularExpressions;
using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a validated email address.
/// RFC 5322 simplified validation is sufficient for a desktop ERP;
/// we reject obviously malformed strings early.
/// </summary>
public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleValidationException("Email.Empty", "Email address is required.");

        var trimmed = email.Trim().ToLowerInvariant();

        // Simplified RFC 5322 regex — good enough for a desktop ERP.
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(trimmed, pattern, RegexOptions.Compiled))
            throw new BusinessRuleValidationException("Email.InvalidFormat", "Email address format is invalid.");

        if (trimmed.Length > 254)
            throw new BusinessRuleValidationException("Email.TooLong", "Email address must not exceed 254 characters.");

        return new Email(trimmed);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
