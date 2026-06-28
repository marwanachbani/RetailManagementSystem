using RMS.Modules.Customers.Domain.Entities;

namespace RMS.Modules.Customers.Application.Contracts;

public sealed record CustomerReadModel(
    Guid Id,
    string CustomerCode,
    string FirstName,
    string LastName,
    string FullName,
    string PhoneNumber,
    string? Email,
    string? Street,
    string? City,
    string? PostalCode,
    string? Country,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    private CustomerReadModel() : this(default, "", "", "", "", "", null, null, null, null, null, "", default, null) { }
}

public sealed record CustomerPurchaseHistoryItem(
    Guid SaleId,
    string SaleNumber,
    DateTime SaleDate,
    decimal TotalAmount,
    string Status)
{
    private CustomerPurchaseHistoryItem() : this(default, "", default, 0, "") { }
}

public sealed record CustomerStatistics(
    Guid CustomerId,
    string CustomerCode,
    string FullName,
    int TotalSales,
    decimal TotalSpent,
    decimal AverageOrderValue,
    DateTime? FirstPurchaseDate,
    DateTime? LastPurchaseDate)
{
    private CustomerStatistics() : this(default, "", "", 0, 0, 0, null, null) { }
}

public interface ICustomerReadStore
{
    Task<CustomerReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerReadModel?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<CustomerReadModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<CustomerReadModel?> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerReadModel>> SearchAsync(string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerReadModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerPurchaseHistoryItem>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerStatistics?> GetStatisticsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> HasSalesAsync(Guid customerId, CancellationToken cancellationToken = default);
}

public interface ICustomerWriteStore
{
    Task InsertAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
