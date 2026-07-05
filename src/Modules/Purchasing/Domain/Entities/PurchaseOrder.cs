using RMS.BuildingBlocks.Domain;

namespace RMS.Modules.Purchasing.Domain.Entities;

public enum PurchaseStatus
{
    Draft = 0,
    Submitted = 1,
    PartiallyReceived = 2,
    Completed = 3,
    Cancelled = 4
}

public sealed class PurchaseOrder : AggregateRoot<Guid>
{
    public string PurchaseNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public string SupplierName { get; private set; } = string.Empty;
    public DateTime OrderDate { get; private set; }
    public PurchaseStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TaxPercentage { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? Notes { get; private set; }
    public string? SupplierInvoiceNumber { get; private set; }

    private readonly List<PurchaseOrderItem> _items = new();
    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    private readonly List<GoodsReceipt> _goodsReceipts = new();
    public IReadOnlyCollection<GoodsReceipt> GoodsReceipts => _goodsReceipts.AsReadOnly();

    private PurchaseOrder() { }

    public static PurchaseOrder Rehydrate(
        Guid id,
        string purchaseNumber,
        Guid supplierId,
        string supplierName,
        DateTime orderDate,
        PurchaseStatus status,
        decimal subTotal,
        decimal taxAmount,
        decimal totalAmount,
        decimal taxPercentage,
        DateTime? completedAt,
        DateTime? cancelledAt,
        DateTime createdAt,
        string? notes,
        string? supplierInvoiceNumber)
    {
        return new PurchaseOrder
        {
            Id = id,
            PurchaseNumber = purchaseNumber,
            SupplierId = supplierId,
            SupplierName = supplierName,
            OrderDate = orderDate,
            Status = status,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            TaxPercentage = taxPercentage,
            CompletedAt = completedAt,
            CancelledAt = cancelledAt,
            CreatedAt = createdAt,
            Notes = notes,
            SupplierInvoiceNumber = supplierInvoiceNumber
        };
    }

    public static PurchaseOrder Create(Guid id, Guid supplierId, string supplierName, string? notes = null)
    {
        var order = new PurchaseOrder
        {
            Id = id,
            PurchaseNumber = GeneratePurchaseNumber(),
            SupplierId = supplierId,
            SupplierName = supplierName,
            OrderDate = DateTime.UtcNow,
            Status = PurchaseStatus.Draft,
            SubTotal = 0,
            TaxAmount = 0,
            TotalAmount = 0,
            TaxPercentage = 0,
            CreatedAt = DateTime.UtcNow,
            Notes = notes
        };

        order.Raise(new Events.PurchaseOrderCreatedEvent(order.Id, order.PurchaseNumber, order.SupplierId, order.OrderDate));
        return order;
    }

    public void UpdateDetails(Guid supplierId, string supplierName, string? notes, decimal taxPercentage)
    {
        EnsureEditable();

        SupplierId = supplierId;
        SupplierName = supplierName;
        Notes = notes;
        TaxPercentage = taxPercentage < 0 ? 0 : taxPercentage;
        RecalculateTotals();

        Raise(new Events.PurchaseOrderUpdatedEvent(Id, PurchaseNumber, SupplierId, OrderDate));
    }

    public void AddItem(Guid productId, string productName, int quantity, decimal unitCost)
    {
        EnsureEditable();

        if (quantity <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.InvalidQuantity", "Quantity must be greater than zero.");
        if (unitCost <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.InvalidUnitCost", "Unit cost must be greater than zero.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = PurchaseOrderItem.Create(Guid.NewGuid(), Id, productId, productName, quantity, unitCost);
            _items.Add(item);
        }

        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureEditable();

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.ItemNotFound", "Purchase order item not found.");

        _items.Remove(item);
        RecalculateTotals();
    }

    public void UpdateItem(Guid itemId, int quantity, decimal unitCost)
    {
        EnsureEditable();

        if (quantity <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.InvalidQuantity", "Quantity must be greater than zero.");
        if (unitCost <= 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.InvalidUnitCost", "Unit cost must be greater than zero.");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.ItemNotFound", "Purchase order item not found.");

        item.UpdateQuantity(quantity);
        item.UpdateUnitCost(unitCost);
        RecalculateTotals();
    }

    public void Submit()
    {
        if (Status != PurchaseStatus.Draft)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.NotDraft", "Only draft purchase orders can be submitted.");
        if (_items.Count == 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.EmptyOrder", "Purchase order must contain at least one item.");

        Status = PurchaseStatus.Submitted;
    }

    public void Cancel()
    {
        if (Status == PurchaseStatus.Cancelled)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.AlreadyCancelled", "Purchase order has already been cancelled.");
        if (Status == PurchaseStatus.Completed)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.AlreadyCompleted", "Completed purchase orders cannot be cancelled.");

        Status = PurchaseStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Raise(new Events.PurchaseOrderCancelledEvent(Id, PurchaseNumber, CancelledAt.Value));
    }

    public void ReceiveGoods(Guid goodsReceiptId, Guid productId, int receivedQuantity, string? batchNumber = null, DateTime? expiryDate = null)
    {
        if (Status == PurchaseStatus.Cancelled)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.Cancelled", "Cancelled purchase orders cannot receive goods.");
        if (Status == PurchaseStatus.Completed)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.AlreadyCompleted", "Completed purchase orders cannot receive goods.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.ItemNotFound", "Product not found on this purchase order.");

        if (item.ReceivedQuantity + receivedQuantity > item.Quantity)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.OverReceipt", "Received quantity cannot exceed ordered quantity.");

        var receipt = GoodsReceipt.Create(goodsReceiptId, Id, productId, receivedQuantity, batchNumber, expiryDate);
        _goodsReceipts.Add(receipt);

        item.IncreaseReceivedQuantity(receivedQuantity);

        Raise(new Events.GoodsReceivedEvent(Id, PurchaseNumber, productId, item.ProductName, receivedQuantity, item.ReceivedQuantity, item.Quantity));

        if (_items.All(i => i.ReceivedQuantity >= i.Quantity))
        {
            Status = PurchaseStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            Raise(new Events.PurchaseCompletedEvent(Id, PurchaseNumber, TotalAmount, CompletedAt.Value));
        }
        else if (_items.Any(i => i.ReceivedQuantity > 0))
        {
            Status = PurchaseStatus.PartiallyReceived;
        }
    }

    public void RecordInvoiceNumber(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.InvalidInvoice", "Invoice number cannot be empty.");

        SupplierInvoiceNumber = invoiceNumber;
    }

    public void Complete()
    {
        if (Status == PurchaseStatus.Cancelled)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.Cancelled", "Cancelled purchase orders cannot be completed.");
        if (Status == PurchaseStatus.Completed)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.AlreadyCompleted", "Purchase order is already completed.");
        if (_items.Count == 0)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.EmptyOrder", "Purchase order must contain at least one item.");

        Status = PurchaseStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Raise(new Events.PurchaseCompletedEvent(Id, PurchaseNumber, TotalAmount, CompletedAt.Value));
    }

    public void RehydrateItems(IEnumerable<PurchaseOrderItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }

    public void RehydrateGoodsReceipts(IEnumerable<GoodsReceipt> receipts)
    {
        _goodsReceipts.Clear();
        _goodsReceipts.AddRange(receipts);
    }

    private void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.TotalCost);
        TaxAmount = SubTotal * (TaxPercentage / 100);
        TotalAmount = SubTotal + TaxAmount;
    }

    private void EnsureEditable()
    {
        if (Status != PurchaseStatus.Draft && Status != PurchaseStatus.Submitted)
            throw new BuildingBlocks.Exceptions.BusinessRuleValidationException("PurchaseOrder.NotEditable", "Only draft or submitted purchase orders can be modified.");
    }

    private static string GeneratePurchaseNumber()
    {
        return $"PO-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
    }
}
