using FluentValidation;

namespace RMS.Modules.Sales.Application.RefundSale;

public sealed class RefundSaleValidator : AbstractValidator<RefundSaleCommand>
{
    public RefundSaleValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
    }
}
