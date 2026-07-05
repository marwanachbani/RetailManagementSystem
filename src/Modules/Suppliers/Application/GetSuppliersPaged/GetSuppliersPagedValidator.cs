using FluentValidation;

namespace RMS.Modules.Suppliers.Application.GetSuppliersPaged;

public sealed class GetSuppliersPagedValidator : AbstractValidator<GetSuppliersPagedQuery>
{
    public GetSuppliersPagedValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
