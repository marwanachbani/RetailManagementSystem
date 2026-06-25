using FluentValidation;

namespace RMS.Modules.Inventory.Application.IncreaseStock;

public sealed class IncreaseStockValidator : AbstractValidator<IncreaseStockCommand>
{
    public IncreaseStockValidator()
    {
        RuleFor(x => x.InventoryItemId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
