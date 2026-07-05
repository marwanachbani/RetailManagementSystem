using FluentValidation;

namespace RMS.Modules.Purchasing.Application.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.SupplierName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TaxPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.Items).NotEmpty().WithMessage("Purchase order must contain at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(150);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitCost).GreaterThan(0);
        });
    }
}
