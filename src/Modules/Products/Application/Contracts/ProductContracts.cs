using RMS.Modules.Products.Domain.Entities;

namespace RMS.Modules.Products.Application.Contracts;

public sealed record ProductReadModel(
    Guid Id,
    string ProductCode,
    string Name,
    string? Description,
    string Barcode,
    Guid CategoryId,
    string CategoryName,
    decimal SalePrice,
    decimal CostPrice,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CategoryReadModel(Guid Id, string Name, string? Description);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public interface IProductReadStore
{
    Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductReadModel?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductReadModel>> SearchAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryReadModel>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}

public interface IProductWriteStore
{
    Task InsertAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
}
