using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventStore;
using RMS.Modules.Suppliers.Application.Contracts;
using RMS.Modules.Suppliers.Domain.Entities;

namespace RMS.Modules.Suppliers.Infrastructure.Persistence;

public sealed class SupplierWriteStore : ISupplierWriteStore
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IEventStore _eventStore;

    public SupplierWriteStore(IDbConnectionFactory connectionFactory, IEventStore eventStore)
    {
        _connectionFactory = connectionFactory;
        _eventStore = eventStore;
    }

    public async Task InsertAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string sql = """
            INSERT INTO Suppliers (Id, SupplierCode, CompanyName, ContactPerson, PhoneNumber, Email, VatNumber, Street, City, PostalCode, Country, Status, CreatedAt, UpdatedAt)
            VALUES (@Id, @SupplierCode, @CompanyName, @ContactPerson, @PhoneNumber, @Email, @VatNumber, @Street, @City, @PostalCode, @Country, @Status, @CreatedAt, @UpdatedAt);
            """;

        await connection.ExecuteAsync(sql, ToParameters(supplier), transaction);
        await AppendEventsAsync(supplier, transaction, cancellationToken);
        transaction.Commit();
    }

    public async Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string sql = """
            UPDATE Suppliers
            SET CompanyName = @CompanyName,
                ContactPerson = @ContactPerson,
                PhoneNumber = @PhoneNumber,
                Email = @Email,
                VatNumber = @VatNumber,
                Street = @Street,
                City = @City,
                PostalCode = @PostalCode,
                Country = @Country,
                Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """;

        await connection.ExecuteAsync(sql, ToParameters(supplier), transaction);
        await AppendEventsAsync(supplier, transaction, cancellationToken);
        transaction.Commit();
    }

    private async Task AppendEventsAsync(Supplier supplier, System.Data.IDbTransaction transaction, CancellationToken cancellationToken)
    {
        var version = 1;
        foreach (var domainEvent in supplier.DomainEvents)
        {
            var storedEvent = SqliteEventStore.CreateStoredEvent(supplier.Id, nameof(Supplier), domainEvent, version++);
            await _eventStore.AppendAsync(storedEvent, transaction, cancellationToken);
        }
    }

    private static object ToParameters(Supplier supplier)
    {
        var address = supplier.Address;
        return new
        {
            Id = supplier.Id.ToString(),
            supplier.SupplierCode,
            supplier.CompanyName,
            supplier.ContactPerson,
            PhoneNumber = supplier.PhoneNumber.Value,
            Email = supplier.Email?.Value,
            supplier.VatNumber,
            Street = address?.Street,
            City = address?.City,
            PostalCode = address?.PostalCode,
            Country = address?.Country,
            Status = (int)supplier.Status,
            CreatedAt = supplier.CreatedAt.ToString("O"),
            UpdatedAt = supplier.UpdatedAt?.ToString("O")
        };
    }
}
