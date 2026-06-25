using FluentValidation;

namespace RMS.Modules.Products.Application.SearchProducts;

public sealed class SearchProductsValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));
    }
}
