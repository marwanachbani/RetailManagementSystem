using RMS.Modules.Suppliers.Domain.Entities;

namespace RMS.Modules.Suppliers.Application.Contracts;

public sealed record SupplierReadModel(
    Guid Id,
    string SupplierCode,
    string CompanyName,
    string? ContactPerson,
    string PhoneNumber,
    string? Email,
    string? VatNumber,
    string? Street,
    string? City,
    string? PostalCode,
    string? Country,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    private SupplierReadModel() : this(default, "", "", null, "", null, null, null, null, null, null, "", default, null) { }
}

public sealed record SupplierProductReadModel(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string CategoryName,
    decimal SalePrice)
{
    private SupplierProductReadModel() : this(default, "", "", "", 0) { }
}

public sealed record SupplierStatisticsReadModel(
    int TotalProducts,
    decimal TotalProductValue,
    DateTime? LastDeliveryDate,
    int? DaysSinceLastDelivery)
{
    private SupplierStatisticsReadModel() : this(0, 0, null, null) { }
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public interface ISupplierReadStore
{
    Task<SupplierReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupplierReadModel>> SearchAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default);
    Task<PagedResult<SupplierReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupplierProductReadModel>> GetSupplierProductsAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<SupplierStatisticsReadModel?> GetStatisticsAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string? email, CancellationToken cancellationToken = default);
    Task<bool> VatNumberExistsAsync(string? vatNumber, CancellationToken cancellationToken = default);
}

public interface ISupplierWriteStore
{
    Task InsertAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
}
