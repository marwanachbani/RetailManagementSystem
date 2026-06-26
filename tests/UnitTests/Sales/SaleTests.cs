using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Sales.Domain.Entities;
using RMS.Modules.Sales.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Sales;

public class SaleTests
{
    [Fact]
    public void Create_Should_InitializeWithPendingStatus()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());

        sale.Status.Should().Be(SaleStatus.Pending);
        sale.SaleNumber.Should().StartWith("SALE-");
        sale.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_Should_RaiseSaleCreatedEvent()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());

        sale.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "SaleCreatedEvent");
    }

    [Fact]
    public void AddItem_Should_AddToItems()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.ClearDomainEvents();

        sale.AddItem(Guid.NewGuid(), "Widget", 3, 10.00m);

        sale.Items.Should().ContainSingle();
        sale.Items.First().ProductName.Should().Be("Widget");
        sale.Items.First().Quantity.Should().Be(3);
        sale.Items.First().UnitPrice.Should().Be(10.00m);
    }

    [Fact]
    public void AddItem_WhenExistingProduct_Should_IncreaseQuantity()
    {
        var productId = Guid.NewGuid();
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(productId, "Widget", 2, 10.00m);
        sale.ClearDomainEvents();

        sale.AddItem(productId, "Widget", 3, 10.00m);

        sale.Items.Should().ContainSingle();
        sale.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_Should_RaiseSaleItemAddedEvent()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.ClearDomainEvents();

        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);

        sale.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "SaleItemAddedEvent");
    }

    [Fact]
    public void AddItem_WithZeroQuantity_Should_Throw()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());

        Action act = () => sale.AddItem(Guid.NewGuid(), "Widget", 0, 10.00m);

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "SaleItem.InvalidQuantity");
    }

    [Fact]
    public void RemoveItem_Should_RemoveFromItems()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Widget", 2, 10.00m);
        sale.ClearDomainEvents();

        var itemId = sale.Items.First().Id;
        sale.RemoveItem(itemId);

        sale.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_Should_RaiseSaleItemRemovedEvent()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.ClearDomainEvents();

        var itemId = sale.Items.First().Id;
        sale.RemoveItem(itemId);

        sale.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "SaleItemRemovedEvent");
    }

    [Fact]
    public void RemoveItem_NotFound_Should_Throw()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());

        Action act = () => sale.RemoveItem(Guid.NewGuid());

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Sale.ItemNotFound");
    }

    [Fact]
    public void Complete_WithItems_Should_SetStatusToCompleted()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.ClearDomainEvents();

        sale.Complete();

        sale.Status.Should().Be(SaleStatus.Completed);
        sale.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_Should_RaiseSaleCompletedEvent()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.ClearDomainEvents();

        sale.Complete();

        sale.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "SaleCompletedEvent");
    }

    [Fact]
    public void Complete_WithoutItems_Should_Throw()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());

        Action act = () => sale.Complete();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Sale.EmptySale");
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_Should_Throw()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.Complete();
        sale.ClearDomainEvents();

        Action act = () => sale.Complete();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Sale.NotEditable");
    }

    [Fact]
    public void Refund_Should_SetStatusToRefunded()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.Complete();
        sale.ClearDomainEvents();

        sale.Refund();

        sale.Status.Should().Be(SaleStatus.Refunded);
        sale.RefundedAt.Should().NotBeNull();
    }

    [Fact]
    public void Refund_Should_RaiseSaleRefundedEvent()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.Complete();
        sale.ClearDomainEvents();

        sale.Refund();

        sale.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "SaleRefundedEvent");
    }

    [Fact]
    public void Refund_WhenNotCompleted_Should_Throw()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.ClearDomainEvents();

        Action act = () => sale.Refund();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Sale.NotCompleted");
    }

    [Fact]
    public void Refund_WhenAlreadyRefunded_Should_Throw()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.Complete();
        sale.Refund();
        sale.ClearDomainEvents();

        Action act = () => sale.Refund();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Sale.AlreadyRefunded");
    }

    [Fact]
    public void ApplyDiscount_Should_ReduceTotal()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 100.00m);
        sale.ClearDomainEvents();

        sale.ApplyDiscount(10);
        sale.Complete();

        sale.DiscountAmount.Should().Be(20.00m);
        sale.TotalAmount.Should().Be(180.00m);
    }

    [Fact]
    public void ApplyTax_Should_IncreaseTotal()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 1, 100.00m);
        sale.ClearDomainEvents();

        sale.ApplyTax(20);
        sale.Complete();

        sale.TaxAmount.Should().Be(20.00m);
        sale.TotalAmount.Should().Be(120.00m);
    }

    [Fact]
    public void ApplyDiscountAndTax_Should_CalculateCorrectly()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 100.00m); // SubTotal = 200
        sale.ClearDomainEvents();

        sale.ApplyDiscount(10); // Discount = 20, after discount = 180
        sale.ApplyTax(20); // Tax = 36, total = 216
        sale.Complete();

        sale.SubTotal.Should().Be(200.00m);
        sale.DiscountAmount.Should().Be(20.00m);
        sale.TaxAmount.Should().Be(36.00m);
        sale.TotalAmount.Should().Be(216.00m);
    }

    [Fact]
    public void AddItem_WhenCompleted_Should_Throw()
    {
        var sale = Sale.Create(Guid.NewGuid(), Guid.NewGuid());
        sale.AddItem(Guid.NewGuid(), "Widget", 2, 10.00m);
        sale.Complete();
        sale.ClearDomainEvents();

        Action act = () => sale.AddItem(Guid.NewGuid(), "Gadget", 1, 5.00m);

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Sale.NotEditable");
    }
}
