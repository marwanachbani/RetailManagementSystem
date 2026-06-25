using FluentValidation;

namespace RMS.Modules.Inventory.Application.DecreaseStock;

public sealed class DecreaseStockValidator : AbstractValidator<DecreaseStockCommand>
{
    public DecreaseStockValidator()
    {
        RuleFor(x => x.InventoryItemId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
