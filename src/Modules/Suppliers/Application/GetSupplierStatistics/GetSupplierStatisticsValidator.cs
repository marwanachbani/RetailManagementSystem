using FluentValidation;

namespace RMS.Modules.Suppliers.Application.GetSupplierStatistics;

public sealed class GetSupplierStatisticsValidator : AbstractValidator<GetSupplierStatisticsQuery>
{
    public GetSupplierStatisticsValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
    }
}
