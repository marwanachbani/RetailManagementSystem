using FluentValidation;

namespace RMS.Modules.Inventory.Application.CreateInventoryItem;

public sealed class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemCommand>
{
    public CreateInventoryItemValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LowStockThreshold).GreaterThanOrEqualTo(0);
    }
}
