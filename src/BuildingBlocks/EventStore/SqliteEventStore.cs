using System.Data;
using System.Text.Json;
using Dapper;
using RMS.BuildingBlocks.Contracts;

namespace RMS.BuildingBlocks.EventStore;

/// <summary>
/// SQLite-backed append-only event store. Stores events as JSON blobs in the EventStore table.
/// This is the concrete implementation of the hybrid event store contract.
/// </summary>
public sealed class SqliteEventStore : IEventStore
{
    private readonly IDbConnectionFactory _connectionFactory;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public SqliteEventStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AppendAsync(StoredEvent storedEvent, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var ownConnection = false;
        IDbConnection? connection = null;

        try
        {
            if (transaction is not null)
            {
                connection = transaction.Connection!;
            }
            else
            {
                connection = _connectionFactory.CreateConnection();
                ownConnection = true;
            }

            const string sql = @"
                INSERT INTO EventStore (EventId, AggregateId, AggregateType, EventType, PayloadJson, OccurredOn, Version)
                VALUES (@EventId, @AggregateId, @AggregateType, @EventType, @PayloadJson, @OccurredOn, @Version);";

            var parameters = new
            {
                storedEvent.EventId,
                storedEvent.AggregateId,
                storedEvent.AggregateType,
                storedEvent.EventType,
                storedEvent.PayloadJson,
                storedEvent.OccurredOn,
                storedEvent.Version
            };

            await connection.ExecuteAsync(sql, parameters, transaction);
        }
        finally
        {
            if (ownConnection)
                connection?.Dispose();
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT EventId, AggregateId, AggregateType, EventType, PayloadJson, OccurredOn, Version
            FROM EventStore
            WHERE AggregateId = @AggregateId
            ORDER BY Version ASC;";

        var rows = await connection.QueryAsync<StoredEvent>(sql, new { AggregateId = aggregateId });
        return rows.ToList().AsReadOnly();
    }

    /// <summary>
    /// Helper to serialize a domain event into a StoredEvent.
    /// </summary>
    public static StoredEvent CreateStoredEvent(Guid aggregateId, string aggregateType, object domainEvent, int version)
    {
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);
        return new StoredEvent(
            Guid.NewGuid(),
            aggregateId,
            aggregateType,
            domainEvent.GetType().FullName!,
            payload,
            DateTime.UtcNow,
            version);
    }
}
