using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Sales.Domain.Events;

public sealed record SaleCreatedEvent(
    Guid SaleId,
    string SaleNumber,
    Guid CashierId,
    DateTime SaleDate) : DomainEvent;

public sealed record SaleItemAddedEvent(
    Guid SaleId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice) : DomainEvent;

public sealed record SaleItemRemovedEvent(
    Guid SaleId,
    Guid ProductId,
    string ProductName,
    int Quantity) : DomainEvent;

public sealed record SaleCompletedEvent(
    Guid SaleId,
    string SaleNumber,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    DateTime CompletedAt) : DomainEvent;

public sealed record SaleRefundedEvent(
    Guid SaleId,
    string SaleNumber,
    decimal RefundAmount,
    DateTime RefundedAt) : DomainEvent;

public sealed record ReceiptGeneratedEvent(
    Guid SaleId,
    Guid ReceiptId,
    string ReceiptNumber) : DomainEvent;
