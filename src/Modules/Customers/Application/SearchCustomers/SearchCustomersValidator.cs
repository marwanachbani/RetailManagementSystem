using FluentValidation;

namespace RMS.Modules.Customers.Application.SearchCustomers;

public sealed class SearchCustomersValidator : AbstractValidator<SearchCustomersQuery>
{
    public SearchCustomersValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("Search term must not exceed 200 characters.");
    }
}
