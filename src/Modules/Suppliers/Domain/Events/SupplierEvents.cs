using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Suppliers.Domain.Events;

public sealed record SupplierCreatedEvent(
    Guid SupplierId,
    string SupplierCode,
    string CompanyName,
    string PhoneNumber,
    string? Email) : DomainEvent;

public sealed record SupplierUpdatedEvent(
    Guid SupplierId,
    string SupplierCode,
    string CompanyName,
    string PhoneNumber) : DomainEvent;

public sealed record SupplierDeactivatedEvent(
    Guid SupplierId,
    string SupplierCode) : DomainEvent;

public sealed record SupplierReactivatedEvent(
    Guid SupplierId,
    string SupplierCode) : DomainEvent;
