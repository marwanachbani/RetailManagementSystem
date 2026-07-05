using FluentValidation;

namespace RMS.Modules.Purchasing.Application.GetPurchaseOrdersPaged;

public sealed class GetPurchaseOrdersPagedValidator : AbstractValidator<GetPurchaseOrdersPagedQuery>
{
    public GetPurchaseOrdersPagedValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
