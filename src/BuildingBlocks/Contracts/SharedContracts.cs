using System.Data;

namespace RMS.BuildingBlocks.Contracts;

/// <summary>
/// Abstraction over creating an open ADO.NET connection to the local SQLite
/// database. Implemented once in the Desktop host's composition root;
/// every module's Infrastructure layer depends only on this interface.
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}

/// <summary>
/// Append-only audit/event record persisted by the Hybrid Event Store.
/// </summary>
public sealed record StoredEvent(
    Guid EventId,
    Guid AggregateId,
    string AggregateType,
    string EventType,
    string PayloadJson,
    DateTime OccurredOn,
    int Version)
{
    private StoredEvent() : this(default, default, "", "", "", default, 0) { }
}

public interface IEventStore
{
    Task AppendAsync(StoredEvent storedEvent, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredEvent>> GetByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken = default);
}
