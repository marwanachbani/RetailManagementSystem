using FluentValidation;

namespace RMS.Modules.Customers.Application.GetCustomerPurchaseHistory;

public sealed class GetCustomerPurchaseHistoryValidator : AbstractValidator<GetCustomerPurchaseHistoryQuery>
{
    public GetCustomerPurchaseHistoryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
