using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Identity.Application.Contracts;
using RMS.Modules.Identity.Domain.Services;

namespace RMS.Modules.Identity.Application.AuthenticateUser;

public sealed class AuthenticateUserHandler : IRequestHandler<AuthenticateUserQuery, Result<AuthenticateUserResult>>
{
    private readonly IUserReadStore _readStore;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticateUserHandler(IUserReadStore readStore, IPasswordHasher passwordHasher)
    {
        _readStore = readStore;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthenticateUserResult>> Handle(AuthenticateUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _readStore.GetByUserNameAsync(request.UserName, cancellationToken);
        if (user is null)
            return Result.Failure<AuthenticateUserResult>("Invalid credentials.", "Identity.InvalidCredentials");

        if (!user.IsActive)
            return Result.Failure<AuthenticateUserResult>("Account is deactivated.", "Identity.AccountDeactivated");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthenticateUserResult>("Invalid credentials.", "Identity.InvalidCredentials");

        return Result.Success(new AuthenticateUserResult(
            user.Id,
            user.UserName,
            user.FullName,
            user.Role));
    }
}
