namespace RMS.Modules.Identity.Domain.Services;

/// <summary>
/// Domain service contract for password hashing.
/// Implementation lives in Infrastructure (BCrypt) so the Domain stays pure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plaintextPassword);
    bool Verify(string plaintextPassword, string hashedPassword);
}
