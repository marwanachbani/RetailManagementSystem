using FluentValidation;

namespace RMS.Modules.Reporting.Application.GetPurchaseReport;

public class GetPurchaseReportValidator : AbstractValidator<GetPurchaseReportQuery>
{
    public GetPurchaseReportValidator()
    {
        RuleFor(x => x.DateRange).ChildRules(dateRange =>
        {
            dateRange.RuleFor(d => d.ToDate).GreaterThanOrEqualTo(d => d.FromDate).When(d => d.FromDate.HasValue && d.ToDate.HasValue);
        });
    }
}
