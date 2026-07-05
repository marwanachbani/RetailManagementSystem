using FluentValidation;

namespace RMS.Modules.Purchasing.Application.SearchPurchaseOrders;

public sealed class SearchPurchaseOrdersValidator : AbstractValidator<SearchPurchaseOrdersQuery>
{
    public SearchPurchaseOrdersValidator()
    {
        RuleFor(x => x.SearchTerm).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}
