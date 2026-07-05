using MediatR;
using RMS.BuildingBlocks.Results;

namespace RMS.Modules.Purchasing.Application.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitCost);

public sealed record CreatePurchaseOrderCommand(
    Guid SupplierId,
    string SupplierName,
    string? Notes,
    decimal TaxPercentage,
    List<CreatePurchaseOrderItemDto> Items) : IRequest<Result<Guid>>;
