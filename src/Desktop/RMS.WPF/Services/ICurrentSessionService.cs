namespace RMS.WPF.Services;

/// <summary>
/// Holds the identity of the currently logged-in operator for the lifetime of the
/// desktop session. Registered as a singleton so every view model resolved after
/// login sees the same signed-in user without having to pass IDs through
/// constructor parameters by hand.
/// </summary>
public interface ICurrentSessionService
{
    Guid UserId { get; }
    string UserName { get; }
    string FullName { get; }
    string Role { get; }
    bool IsAuthenticated { get; }

    void SignIn(Guid userId, string userName, string fullName, string role);
    void SignOut();
}

public sealed class CurrentSessionService : ICurrentSessionService
{
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool IsAuthenticated => UserId != Guid.Empty;

    public void SignIn(Guid userId, string userName, string fullName, string role)
    {
        UserId = userId;
        UserName = userName;
        FullName = fullName;
        Role = role;
    }

    public void SignOut()
    {
        UserId = Guid.Empty;
        UserName = string.Empty;
        FullName = string.Empty;
        Role = string.Empty;
    }
}
