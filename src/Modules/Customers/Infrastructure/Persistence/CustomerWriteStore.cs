using System.Data;
using Dapper;
using RMS.BuildingBlocks.Contracts;
using RMS.BuildingBlocks.EventStore;
using RMS.Modules.Customers.Application.Contracts;
using RMS.Modules.Customers.Domain.Entities;

namespace RMS.Modules.Customers.Infrastructure.Persistence;

public sealed class CustomerWriteStore : ICustomerWriteStore
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IEventStore _eventStore;

    public CustomerWriteStore(IDbConnectionFactory connectionFactory, IEventStore eventStore)
    {
        _connectionFactory = connectionFactory;
        _eventStore = eventStore;
    }

    public async Task InsertAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string sql = """
            INSERT INTO Customers (Id, CustomerCode, FirstName, LastName, PhoneNumber, Email,
                                   Street, City, PostalCode, Country, Status, CreatedAt, UpdatedAt)
            VALUES (@Id, @CustomerCode, @FirstName, @LastName, @PhoneNumber, @Email,
                    @Street, @City, @PostalCode, @Country, @Status, @CreatedAt, @UpdatedAt);
            """;

        await connection.ExecuteAsync(sql, ToParameters(customer), transaction);
        await AppendEventsAsync(customer, transaction, cancellationToken);
        transaction.Commit();
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        const string sql = """
            UPDATE Customers
            SET CustomerCode = @CustomerCode, FirstName = @FirstName, LastName = @LastName,
                PhoneNumber = @PhoneNumber, Email = @Email, Street = @Street, City = @City,
                PostalCode = @PostalCode, Country = @Country, Status = @Status,
                CreatedAt = @CreatedAt, UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """;

        await connection.ExecuteAsync(sql, ToParameters(customer), transaction);
        await AppendEventsAsync(customer, transaction, cancellationToken);
        transaction.Commit();
    }

    private async Task AppendEventsAsync(Customer customer, IDbTransaction transaction, CancellationToken cancellationToken)
    {
        var version = 1;
        foreach (var domainEvent in customer.DomainEvents)
        {
            var storedEvent = SqliteEventStore.CreateStoredEvent(customer.Id, nameof(Customer), domainEvent, version++);
            await _eventStore.AppendAsync(storedEvent, transaction, cancellationToken);
        }
    }

    private static object ToParameters(Customer customer) => new
    {
        Id = customer.Id,
        CustomerCode = customer.CustomerCode,
        FirstName = customer.FirstName,
        LastName = customer.LastName,
        PhoneNumber = customer.PhoneNumber.Value,
        Email = customer.Email?.Value,
        Street = customer.Address?.Street,
        City = customer.Address?.City,
        PostalCode = customer.Address?.PostalCode,
        Country = customer.Address?.Country,
        Status = (int)customer.Status,
        CreatedAt = customer.CreatedAt.ToString("O"),
        UpdatedAt = customer.UpdatedAt?.ToString("O")
    };
}
