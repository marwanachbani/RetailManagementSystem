using FluentValidation;

namespace RMS.Modules.Reporting.Application.GetSalesReport;

public class GetSalesReportValidator : AbstractValidator<GetSalesReportQuery>
{
    public GetSalesReportValidator()
    {
        RuleFor(x => x.DateRange).ChildRules(dateRange =>
        {
            dateRange.RuleFor(d => d.ToDate).GreaterThanOrEqualTo(d => d.FromDate).When(d => d.FromDate.HasValue && d.ToDate.HasValue);
        });
    }
}
