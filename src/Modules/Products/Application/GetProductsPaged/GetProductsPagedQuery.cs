using MediatR;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Products.Application.Contracts;

namespace RMS.Modules.Products.Application.GetProductsPaged;

public sealed record GetProductsPagedQuery(int PageNumber, int PageSize, string? SearchTerm, bool IncludeInactive = false)
    : IRequest<Result<PagedResult<ProductReadModel>>>;
