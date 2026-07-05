using FluentValidation;

namespace RMS.Modules.Suppliers.Application.GetSupplierProducts;

public sealed class GetSupplierProductsValidator : AbstractValidator<GetSupplierProductsQuery>
{
    public GetSupplierProductsValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
    }
}
