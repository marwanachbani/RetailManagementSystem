using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Identity.Domain.Events;

/// <summary>
/// Raised when a new user is successfully registered.
/// Other modules (e.g., Reporting, Audit) can subscribe to the corresponding
/// integration event via the InProcessEventBus without referencing Identity directly.
/// </summary>
public sealed record UserRegisteredEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string Email { get; }
    public string FullName { get; }
    public string Role { get; }
    public DateTime RegisteredAt { get; }

    public UserRegisteredEvent(Guid userId, string userName, string email, string fullName, string role, DateTime registeredAt)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
        FullName = fullName;
        Role = role;
        RegisteredAt = registeredAt;
    }
}
