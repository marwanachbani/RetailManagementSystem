using FluentValidation;

namespace RMS.Modules.Suppliers.Application.GetSupplierById;

public sealed class GetSupplierByIdValidator : AbstractValidator<GetSupplierByIdQuery>
{
    public GetSupplierByIdValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
    }
}
