using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Identity.Application.RegisterUser;

/// <summary>
/// Command to register a new user in the system.
/// </summary>
public sealed record RegisterUserCommand(
    string UserName,
    string Email,
    string Password,
    string FullName,
    string Role) : IRequest<Result<Guid>>;
