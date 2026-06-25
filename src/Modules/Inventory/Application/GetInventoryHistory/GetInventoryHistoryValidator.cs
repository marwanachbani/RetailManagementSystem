using FluentValidation;

namespace RMS.Modules.Inventory.Application.GetInventoryHistory;

public sealed class GetInventoryHistoryValidator : AbstractValidator<GetInventoryHistoryQuery>
{
    public GetInventoryHistoryValidator()
    {
        RuleFor(x => x.InventoryItemId).NotEmpty();
    }
}
