using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Customers.Domain.Entities;

public sealed record CustomerCreatedEvent(
    Guid CustomerId,
    string CustomerCode,
    string FullName,
    string PhoneNumber,
    string? Email) : DomainEvent;

public sealed record CustomerUpdatedEvent(
    Guid CustomerId,
    string CustomerCode,
    string FullName,
    string PhoneNumber,
    string? Email) : DomainEvent;

public sealed record CustomerDeactivatedEvent(
    Guid CustomerId,
    string CustomerCode,
    string FullName) : DomainEvent;

public sealed record CustomerReactivatedEvent(
    Guid CustomerId,
    string CustomerCode,
    string FullName) : DomainEvent;
