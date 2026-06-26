using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.EventBus;

namespace RMS.Modules.Sales.Application;

public sealed record SaleCreatedIntegrationEvent(
    Guid SaleId,
    string SaleNumber,
    Guid CashierId) : DomainEvent, IIntegrationEvent;

public sealed record SaleCompletedIntegrationEvent(
    Guid SaleId,
    string SaleNumber,
    decimal TotalAmount) : DomainEvent, IIntegrationEvent;

public sealed record StockReductionRequestedEvent(
    Guid SaleId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    string Reason) : DomainEvent, IIntegrationEvent;

public sealed record StockRestorationRequestedEvent(
    Guid SaleId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    string Reason) : DomainEvent, IIntegrationEvent;

public sealed record SaleRefundedIntegrationEvent(
    Guid SaleId,
    string SaleNumber,
    decimal RefundAmount) : DomainEvent, IIntegrationEvent;
