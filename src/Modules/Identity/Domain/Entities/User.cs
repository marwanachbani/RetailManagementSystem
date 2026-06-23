using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Identity.Domain.Events;
using RMS.Modules.Identity.Domain.ValueObjects;

namespace RMS.Modules.Identity.Domain.Entities;

public enum UserRole
{
    Admin,
    Manager,
    Cashier
}

/// <summary>
/// User aggregate root. Enforces identity invariants at construction time.
/// </summary>
public sealed class User : AggregateRoot<Guid>
{
    public string UserName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { } // Dapper / hydration

    public static User Create(
        Guid id,
        string userName,
        Email email,
        PasswordHash passwordHash,
        string fullName,
        UserRole role = UserRole.Cashier)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new BusinessRuleValidationException("User.UserNameEmpty", "User name is required.");

        if (userName.Length < 3)
            throw new BusinessRuleValidationException("User.UserNameTooShort", "User name must be at least 3 characters.");

        if (userName.Length > 50)
            throw new BusinessRuleValidationException("User.UserNameTooLong", "User name must not exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleValidationException("User.FullNameEmpty", "Full name is required.");

        if (fullName.Length > 100)
            throw new BusinessRuleValidationException("User.FullNameTooLong", "Full name must not exceed 100 characters.");

        var user = new User
        {
            Id = id,
            UserName = userName.Trim(),
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.Raise(new UserRegisteredEvent(
            user.Id,
            user.UserName,
            user.Email.Value,
            user.FullName,
            user.Role.ToString(),
            user.CreatedAt));

        return user;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void UpdateProfile(string fullName, Email email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleValidationException("User.FullNameEmpty", "Full name is required.");

        FullName = fullName.Trim();
        Email = email;
    }

    public void ChangePassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }
}
