using FluentValidation;

namespace RMS.Modules.Inventory.Application.AdjustStock;

public sealed class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.InventoryItemId).NotEmpty();
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
