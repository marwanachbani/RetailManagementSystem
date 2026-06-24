using FluentValidation;

namespace RMS.Modules.Products.Application.DeactivateProduct;

public sealed class DeactivateProductValidator : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
