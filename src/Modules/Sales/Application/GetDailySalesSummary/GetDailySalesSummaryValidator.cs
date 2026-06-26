using FluentValidation;

namespace RMS.Modules.Sales.Application.GetDailySalesSummary;

public sealed class GetDailySalesSummaryValidator : AbstractValidator<GetDailySalesSummaryQuery>
{
    public GetDailySalesSummaryValidator()
    {
        RuleFor(x => x.Date).NotEmpty();
    }
}
