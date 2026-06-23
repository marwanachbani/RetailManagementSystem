using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;

namespace RMS.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Immutable value object wrapping a pre-hashed password.
/// The domain never sees plaintext; hashing is an application/infrastructure concern.
/// </summary>
public sealed class PasswordHash : ValueObject
{
    public string Value { get; }

    private PasswordHash(string value) => Value = value;

    public static PasswordHash Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new BusinessRuleValidationException("PasswordHash.Empty", "Password hash cannot be empty.");

        return new PasswordHash(hash);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
