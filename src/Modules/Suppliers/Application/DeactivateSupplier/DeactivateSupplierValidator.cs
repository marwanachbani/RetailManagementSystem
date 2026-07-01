using FluentValidation;

namespace RMS.Modules.Suppliers.Application.DeactivateSupplier;

public sealed class DeactivateSupplierValidator : AbstractValidator<DeactivateSupplierCommand>
{
    public DeactivateSupplierValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
    }
}
