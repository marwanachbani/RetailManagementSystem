using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Purchasing.Application.UpdatePurchaseOrder;

public sealed record UpdatePurchaseOrderItemDto(
    Guid? Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitCost);

public sealed record UpdatePurchaseOrderCommand(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string SupplierName,
    string? Notes,
    decimal TaxPercentage,
    List<UpdatePurchaseOrderItemDto> Items) : IRequest<Result>;
