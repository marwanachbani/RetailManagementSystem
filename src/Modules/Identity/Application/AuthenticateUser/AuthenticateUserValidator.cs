using FluentValidation;

namespace RMS.Modules.Identity.Application.AuthenticateUser;

public sealed class AuthenticateUserValidator : AbstractValidator<AuthenticateUserQuery>
{
    public AuthenticateUserValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
