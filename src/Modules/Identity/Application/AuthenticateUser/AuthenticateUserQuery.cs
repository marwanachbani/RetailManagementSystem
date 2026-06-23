using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Identity.Application.AuthenticateUser;

/// <summary>
/// Query to authenticate a user and return their identity claims.
/// </summary>
public sealed record AuthenticateUserQuery(
    string UserName,
    string Password) : IRequest<Result<AuthenticateUserResult>>;

public sealed record AuthenticateUserResult(
    Guid UserId,
    string UserName,
    string FullName,
    string Role);
