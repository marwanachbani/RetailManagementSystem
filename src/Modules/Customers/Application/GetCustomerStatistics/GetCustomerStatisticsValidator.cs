using FluentValidation;

namespace RMS.Modules.Customers.Application.GetCustomerStatistics;

public sealed class GetCustomerStatisticsValidator : AbstractValidator<GetCustomerStatisticsQuery>
{
    public GetCustomerStatisticsValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
