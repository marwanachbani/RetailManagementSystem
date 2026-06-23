using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Identity.Application.Contracts;
using RMS.Modules.Identity.Domain.Entities;
using RMS.Modules.Identity.Domain.Services;
using RMS.Modules.Identity.Domain.ValueObjects;

namespace RMS.Modules.Identity.Application.RegisterUser;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUserReadStore _readStore;
    private readonly IUserWriteStore _writeStore;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(
        IUserReadStore readStore,
        IUserWriteStore writeStore,
        IPasswordHasher passwordHasher)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Check uniqueness via read model (CQRS read side).
        var existingByEmail = await _readStore.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null)
            return Result.Failure<Guid>("A user with this email already exists.", "Identity.EmailAlreadyExists");

        var existingByUserName = await _readStore.GetByUserNameAsync(request.UserName, cancellationToken);
        if (existingByUserName is not null)
            return Result.Failure<Guid>("A user with this user name already exists.", "Identity.UserNameAlreadyExists");

        // 2. Hash password — infrastructure concern, never touches domain directly.
        var hashed = _passwordHasher.Hash(request.Password);
        var passwordHash = PasswordHash.Create(hashed);

        // 3. Build value objects (validation happens here).
        var email = Email.Create(request.Email);

        // 4. Parse role safely.
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            role = UserRole.Cashier;

        // 5. Create aggregate — business rules enforced inside.
        var user = User.Create(
            Guid.NewGuid(),
            request.UserName,
            email,
            passwordHash,
            request.FullName,
            role);

        // 6. Persist via write side.
        await _writeStore.InsertAsync(user, cancellationToken);

        // 7. Clear domain events so they are not re-published.
        // In a full event-driven pipeline, we would map these to integration events
        // and push them through IEventBus here. Sprint 2 will wire that fully.
        user.ClearDomainEvents();

        return Result.Success(user.Id);
    }
}
