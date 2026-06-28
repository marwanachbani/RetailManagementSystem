using FluentValidation;

namespace RMS.Modules.Customers.Application.ReactivateCustomer;

public sealed class ReactivateCustomerValidator : AbstractValidator<ReactivateCustomerCommand>
{
    public ReactivateCustomerValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
