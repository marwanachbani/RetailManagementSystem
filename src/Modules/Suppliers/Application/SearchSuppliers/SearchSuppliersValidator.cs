using FluentValidation;

namespace RMS.Modules.Suppliers.Application.SearchSuppliers;

public sealed class SearchSuppliersValidator : AbstractValidator<SearchSuppliersQuery>
{
    public SearchSuppliersValidator()
    {
        RuleFor(x => x.SearchTerm).MaximumLength(200).When(x => x.SearchTerm is not null);
    }
}
