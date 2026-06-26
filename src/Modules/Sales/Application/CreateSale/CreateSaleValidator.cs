using FluentValidation;

namespace RMS.Modules.Sales.Application.CreateSale;

public sealed class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.CashierId).NotEmpty();
    }
}
