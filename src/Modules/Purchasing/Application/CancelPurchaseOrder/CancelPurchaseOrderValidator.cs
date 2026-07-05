using FluentValidation;

namespace RMS.Modules.Purchasing.Application.CancelPurchaseOrder;

public sealed class CancelPurchaseOrderValidator : AbstractValidator<CancelPurchaseOrderCommand>
{
    public CancelPurchaseOrderValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
    }
}
