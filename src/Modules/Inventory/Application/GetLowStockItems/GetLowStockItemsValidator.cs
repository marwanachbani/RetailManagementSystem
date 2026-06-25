using FluentValidation;

namespace RMS.Modules.Inventory.Application.GetLowStockItems;

public sealed class GetLowStockItemsValidator : AbstractValidator<GetLowStockItemsQuery>
{
    public GetLowStockItemsValidator()
    {
        RuleFor(x => x.Threshold).GreaterThanOrEqualTo(0);
    }
}
