using FluentValidation;

namespace RMS.Modules.Suppliers.Application.ReactivateSupplier;

public sealed class ReactivateSupplierValidator : AbstractValidator<ReactivateSupplierCommand>
{
    public ReactivateSupplierValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
    }
}
