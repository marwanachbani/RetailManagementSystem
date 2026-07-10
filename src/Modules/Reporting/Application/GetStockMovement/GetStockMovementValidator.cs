using FluentValidation;

namespace RMS.Modules.Reporting.Application.GetStockMovement;

public class GetStockMovementValidator : AbstractValidator<GetStockMovementQuery>
{
    public GetStockMovementValidator()
    {
        RuleFor(x => x.DateRange).ChildRules(dateRange =>
        {
            dateRange.RuleFor(d => d.ToDate).GreaterThanOrEqualTo(d => d.FromDate).When(d => d.FromDate.HasValue && d.ToDate.HasValue);
        });
    }
}
