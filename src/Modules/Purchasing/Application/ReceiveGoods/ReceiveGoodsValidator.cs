using FluentValidation;

namespace RMS.Modules.Purchasing.Application.ReceiveGoods;

public sealed class ReceiveGoodsValidator : AbstractValidator<ReceiveGoodsCommand>
{
    public ReceiveGoodsValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.QuantityReceived).GreaterThan(0);
    }
}
