using FluentValidation;

namespace RMS.Modules.Products.Application.GetProductsPaged;

public sealed class GetProductsPagedValidator : AbstractValidator<GetProductsPagedQuery>
{
    public GetProductsPagedValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
