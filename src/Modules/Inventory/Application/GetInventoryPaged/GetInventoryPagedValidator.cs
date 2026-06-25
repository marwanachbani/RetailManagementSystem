using FluentValidation;

namespace RMS.Modules.Inventory.Application.GetInventoryPaged;

public sealed class GetInventoryPagedValidator : AbstractValidator<GetInventoryPagedQuery>
{
    public GetInventoryPagedValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
