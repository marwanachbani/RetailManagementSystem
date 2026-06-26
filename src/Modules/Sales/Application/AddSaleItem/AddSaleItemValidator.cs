using FluentValidation;

namespace RMS.Modules.Sales.Application.AddSaleItem;

public sealed class AddSaleItemValidator : AbstractValidator<AddSaleItemCommand>
{
    public AddSaleItemValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
