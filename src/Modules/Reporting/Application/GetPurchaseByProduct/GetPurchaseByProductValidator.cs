using FluentValidation;

namespace RMS.Modules.Reporting.Application.GetPurchaseByProduct;

public class GetPurchaseByProductValidator : AbstractValidator<GetPurchaseByProductQuery>
{
    public GetPurchaseByProductValidator()
    {
        RuleFor(x => x.DateRange).ChildRules(dateRange =>
        {
            dateRange.RuleFor(d => d.ToDate).GreaterThanOrEqualTo(d => d.FromDate).When(d => d.FromDate.HasValue && d.ToDate.HasValue);
        });
    }
}
