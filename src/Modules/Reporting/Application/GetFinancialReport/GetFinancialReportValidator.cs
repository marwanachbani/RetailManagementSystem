using FluentValidation;

namespace RMS.Modules.Reporting.Application.GetFinancialReport;

public class GetFinancialReportValidator : AbstractValidator<GetFinancialReportQuery>
{
    public GetFinancialReportValidator()
    {
        RuleFor(x => x.DateRange).ChildRules(dateRange =>
        {
            dateRange.RuleFor(d => d.ToDate).GreaterThanOrEqualTo(d => d.FromDate).When(d => d.FromDate.HasValue && d.ToDate.HasValue);
        });
        RuleFor(x => x.PeriodType).NotEmpty();
    }
}
