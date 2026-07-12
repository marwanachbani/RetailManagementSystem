using RMS.BuildingBlocks.Contracts;
using RMS.WPF.Services;

namespace RMS.WPF.Services;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly ICurrentSessionService _sessionService;

    public CurrentUserContext(ICurrentSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public Guid? UserId => _sessionService.IsAuthenticated ? _sessionService.UserId : null;
    public string? UserName => _sessionService.IsAuthenticated ? _sessionService.UserName : null;
    public bool IsAuthenticated => _sessionService.IsAuthenticated;
}
