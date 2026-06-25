using FluentValidation;

namespace RMS.Modules.Inventory.Application.GetInventoryItem;

public sealed class GetInventoryItemValidator : AbstractValidator<GetInventoryItemQuery>
{
    public GetInventoryItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
