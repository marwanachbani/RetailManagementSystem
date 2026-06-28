using FluentValidation;

namespace RMS.Modules.Customers.Application.GetCustomerById;

public sealed class GetCustomerByIdValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
