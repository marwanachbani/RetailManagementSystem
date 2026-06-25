using FluentValidation;

namespace RMS.Modules.Products.Application.GetCategories;

public sealed class GetCategoriesValidator : AbstractValidator<GetCategoriesQuery>
{
    public GetCategoriesValidator()
    {
        // GetCategoriesQuery has no parameters to validate.
    }
}
