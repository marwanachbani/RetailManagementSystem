using FluentValidation;

namespace RMS.Modules.Customers.Application.DeactivateCustomer;

public sealed class DeactivateCustomerValidator : AbstractValidator<DeactivateCustomerCommand>
{
    public DeactivateCustomerValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
