using FluentValidation;

namespace RMS.Modules.Purchasing.Application.GetPurchaseOrder;

public sealed class GetPurchaseOrderValidator : AbstractValidator<GetPurchaseOrderQuery>
{
    public GetPurchaseOrderValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
    }
}
