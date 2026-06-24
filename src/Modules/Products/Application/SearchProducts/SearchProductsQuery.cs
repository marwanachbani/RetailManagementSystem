using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;

namespace RMS.Modules.Products.Application.SearchProducts;

public sealed record SearchProductsQuery(string? SearchTerm, bool IncludeInactive = false)
    : IRequest<Result<IReadOnlyList<ProductReadModel>>>;
