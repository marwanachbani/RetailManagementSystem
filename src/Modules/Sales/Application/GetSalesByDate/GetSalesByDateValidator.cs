using FluentValidation;

namespace RMS.Modules.Sales.Application.GetSalesByDate;

public sealed class GetSalesByDateValidator : AbstractValidator<GetSalesByDateQuery>
{
    public GetSalesByDateValidator()
    {
        RuleFor(x => x.Date).NotEmpty();
    }
}
