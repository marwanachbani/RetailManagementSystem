using FluentValidation;

namespace RMS.Modules.Sales.Application.CompleteSale;

public sealed class CompleteSaleValidator : AbstractValidator<CompleteSaleCommand>
{
    public CompleteSaleValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
        RuleFor(x => x.DiscountPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.TaxPercentage).GreaterThanOrEqualTo(0);
    }
}
