using FluentValidation;

namespace RMS.Modules.Sales.Application.GetSaleById;

public sealed class GetSaleByIdValidator : AbstractValidator<GetSaleByIdQuery>
{
    public GetSaleByIdValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
    }
}
