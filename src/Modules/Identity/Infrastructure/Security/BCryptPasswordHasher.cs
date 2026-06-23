using RMS.Modules.Identity.Domain.Services;

namespace RMS.Modules.Identity.Infrastructure.Security;

/// <summary>
/// BCrypt-based password hasher. Implementation detail — the domain only knows IPasswordHasher.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    // Work factor of 12 is a sensible default for a desktop ERP (balanced between security and speed).
    private const int WorkFactor = 12;

    public string Hash(string plaintextPassword)
    {
        if (string.IsNullOrWhiteSpace(plaintextPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(plaintextPassword));

        return BCrypt.Net.BCrypt.HashPassword(plaintextPassword, WorkFactor);
    }

    public bool Verify(string plaintextPassword, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(plaintextPassword) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        return BCrypt.Net.BCrypt.Verify(plaintextPassword, hashedPassword);
    }
}
