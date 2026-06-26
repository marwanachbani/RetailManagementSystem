using FluentValidation;

namespace RMS.Modules.Sales.Application.RemoveSaleItem;

public sealed class RemoveSaleItemValidator : AbstractValidator<RemoveSaleItemCommand>
{
    public RemoveSaleItemValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
        RuleFor(x => x.SaleItemId).NotEmpty();
    }
}
