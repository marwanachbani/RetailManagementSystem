using FluentValidation;

namespace RMS.Modules.Purchasing.Application.CompletePurchase;

public sealed class CompletePurchaseValidator : AbstractValidator<CompletePurchaseCommand>
{
    public CompletePurchaseValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
    }
}
