using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Identity.Application.Contracts;

/// <summary>
/// Lightweight read-only DTO returned by the read side. No domain behavior — just data.
/// </summary>
public sealed record UserReadModel(
    Guid Id,
    string UserName,
    string Email,
    string PasswordHash,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt);

public interface IUserReadStore
{
    Task<UserReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserReadModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserReadModel?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserReadModel>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IUserWriteStore
{
    Task InsertAsync(Domain.Entities.User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.User user, CancellationToken cancellationToken = default);
}
