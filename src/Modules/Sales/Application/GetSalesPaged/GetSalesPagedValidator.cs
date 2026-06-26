using FluentValidation;

namespace RMS.Modules.Sales.Application.GetSalesPaged;

public sealed class GetSalesPagedValidator : AbstractValidator<GetSalesPagedQuery>
{
    public GetSalesPagedValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
