using RMS.BuildingBlocks.Domain;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Sales.Domain.Events;
using RMS.Modules.Sales.Domain.ValueObjects;

namespace RMS.Modules.Sales.Domain.Entities;

public enum SaleStatus
{
    Pending = 0,
    Completed = 1,
    Refunded = 2
}

public sealed class Sale : AggregateRoot<Guid>
{
    public string SaleNumber { get; private set; } = string.Empty;
    public Guid CashierId { get; private set; }
    public DateTime SaleDate { get; private set; }
    public SaleStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal TaxPercentage { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private Sale() { }

    public static Sale Rehydrate(
        Guid id,
        string saleNumber,
        Guid cashierId,
        DateTime saleDate,
        SaleStatus status,
        decimal subTotal,
        decimal discountAmount,
        decimal taxAmount,
        decimal totalAmount,
        decimal discountPercentage,
        decimal taxPercentage,
        DateTime? completedAt,
        DateTime? refundedAt,
        DateTime createdAt,
        string? notes)
    {
        return new Sale
        {
            Id = id,
            SaleNumber = saleNumber,
            CashierId = cashierId,
            SaleDate = saleDate,
            Status = status,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            DiscountPercentage = discountPercentage,
            TaxPercentage = taxPercentage,
            CompletedAt = completedAt,
            RefundedAt = refundedAt,
            CreatedAt = createdAt,
            Notes = notes
        };
    }

    public static Sale Create(Guid id, Guid cashierId, string? notes = null)
    {
        var sale = new Sale
        {
            Id = id,
            SaleNumber = GenerateSaleNumber(),
            CashierId = cashierId,
            SaleDate = DateTime.UtcNow,
            Status = SaleStatus.Pending,
            SubTotal = 0,
            DiscountAmount = 0,
            TaxAmount = 0,
            TotalAmount = 0,
            DiscountPercentage = 0,
            TaxPercentage = 0,
            CreatedAt = DateTime.UtcNow,
            Notes = notes
        };

        sale.Raise(new SaleCreatedEvent(sale.Id, sale.SaleNumber, sale.CashierId, sale.SaleDate));
        return sale;
    }

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        EnsurePending();

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = SaleItem.Create(Guid.NewGuid(), Id, productId, productName, quantity, unitPrice);
            _items.Add(item);
        }

        RecalculateTotals();
        Raise(new SaleItemAddedEvent(Id, productId, productName, quantity, unitPrice));
    }

    public void RemoveItem(Guid saleItemId)
    {
        EnsurePending();

        var item = _items.FirstOrDefault(i => i.Id == saleItemId);
        if (item is null)
            throw new BusinessRuleValidationException("Sale.ItemNotFound", "Sale item not found.");

        _items.Remove(item);
        RecalculateTotals();
        Raise(new SaleItemRemovedEvent(Id, item.ProductId, item.ProductName, item.Quantity));
    }

    public void ApplyDiscount(decimal discountPercentage)
    {
        EnsurePending();
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new BusinessRuleValidationException("Sale.InvalidDiscount", "Discount percentage must be between 0 and 100.");
        
        DiscountPercentage = discountPercentage;
        RecalculateTotals();
    }

    public void ApplyTax(decimal taxPercentage)
    {
        EnsurePending();
        if (taxPercentage < 0)
            throw new BusinessRuleValidationException("Sale.InvalidTax", "Tax percentage cannot be negative.");
        
        TaxPercentage = taxPercentage;
        RecalculateTotals();
    }

    public void Complete()
    {
        EnsurePending();
        if (_items.Count == 0)
            throw new BusinessRuleValidationException("Sale.EmptySale", "Sale must contain at least one item.");

        Status = SaleStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Raise(new SaleCompletedEvent(Id, SaleNumber, TotalAmount, DiscountAmount, TaxAmount, CompletedAt.Value));
    }

    public void Refund(string? reason = null)
    {
        if (Status == SaleStatus.Refunded)
            throw new BusinessRuleValidationException("Sale.AlreadyRefunded", "Sale has already been refunded.");
        if (Status != SaleStatus.Completed)
            throw new BusinessRuleValidationException("Sale.NotCompleted", "Only completed sales can be refunded.");

        Status = SaleStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        Raise(new SaleRefundedEvent(Id, SaleNumber, TotalAmount, RefundedAt.Value));
    }

    public void RehydrateItems(IEnumerable<SaleItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }

    private void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.TotalPrice);
        DiscountAmount = SubTotal * (DiscountPercentage / 100);
        var afterDiscount = SubTotal - DiscountAmount;
        TaxAmount = afterDiscount * (TaxPercentage / 100);
        TotalAmount = afterDiscount + TaxAmount;
    }

    private void EnsurePending()
    {
        if (Status != SaleStatus.Pending)
            throw new BusinessRuleValidationException("Sale.NotEditable", "Only pending sales can be modified.");
    }

    private static string GenerateSaleNumber()
    {
        return $"SALE-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
    }
}
