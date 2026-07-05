using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Purchasing.Domain.Entities;
using Xunit;

namespace RMS.UnitTests.Purchasing;

public class PurchaseOrderAggregateTests
{
    [Fact]
    public void Create_Should_Generate_PurchaseNumber()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.PurchaseNumber.Should().NotBeNullOrWhiteSpace();
        order.PurchaseNumber.Should().StartWith("PO-");
    }

    [Fact]
    public void Create_Should_Set_Status_To_Draft()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.Status.Should().Be(PurchaseStatus.Draft);
    }

    [Fact]
    public void AddItem_With_Valid_Data_Should_Increase_Item_Count()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(Guid.NewGuid(), "Product A", 5, 10.00m);
        order.Items.Count.Should().Be(1);
        order.SubTotal.Should().Be(50.00m);
    }

    [Fact]
    public void AddItem_With_Zero_Quantity_Should_Throw()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        var act = () => order.AddItem(Guid.NewGuid(), "Product A", 0, 10.00m);
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void AddItem_With_Zero_UnitCost_Should_Throw()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        var act = () => order.AddItem(Guid.NewGuid(), "Product A", 5, 0.00m);
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Submit_With_Items_Should_Set_Status_To_Submitted()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(Guid.NewGuid(), "Product A", 5, 10.00m);
        order.Submit();
        order.Status.Should().Be(PurchaseStatus.Submitted);
    }

    [Fact]
    public void Submit_Without_Items_Should_Throw()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        var act = () => order.Submit();
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Cancel_Should_Set_Status_To_Cancelled()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(Guid.NewGuid(), "Product A", 5, 10.00m);
        order.Submit();
        order.Cancel();
        order.Status.Should().Be(PurchaseStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_Already_Cancelled_Should_Throw()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(Guid.NewGuid(), "Product A", 5, 10.00m);
        order.Submit();
        order.Cancel();
        var act = () => order.Cancel();
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Cancel_Completed_Should_Throw()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(Guid.NewGuid(), "Product A", 5, 10.00m);
        order.Submit();
        order.Complete();
        var act = () => order.Cancel();
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void ReceiveGoods_Should_Increase_ReceivedQuantity()
    {
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(productId, "Product A", 10, 10.00m);
        order.Submit();
        order.ReceiveGoods(Guid.NewGuid(), productId, 5);
        order.Items.First().ReceivedQuantity.Should().Be(5);
    }

    [Fact]
    public void ReceiveGoods_Over_Ordered_Quantity_Should_Throw()
    {
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(productId, "Product A", 5, 10.00m);
        order.Submit();
        var act = () => order.ReceiveGoods(Guid.NewGuid(), productId, 6);
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void ReceiveGoods_Fully_Should_Set_Status_To_Completed()
    {
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(productId, "Product A", 5, 10.00m);
        order.Submit();
        order.ReceiveGoods(Guid.NewGuid(), productId, 5);
        order.Status.Should().Be(PurchaseStatus.Completed);
        order.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void ReceiveGoods_Partially_Should_Set_Status_To_PartiallyReceived()
    {
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(productId, "Product A", 10, 10.00m);
        order.Submit();
        order.ReceiveGoods(Guid.NewGuid(), productId, 3);
        order.Status.Should().Be(PurchaseStatus.PartiallyReceived);
    }

    [Fact]
    public void ReceiveGoods_On_Cancelled_Should_Throw()
    {
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(productId, "Product A", 5, 10.00m);
        order.Submit();
        order.Cancel();
        var act = () => order.ReceiveGoods(Guid.NewGuid(), productId, 1);
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Complete_Should_Set_Status_To_Completed()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(Guid.NewGuid(), "Product A", 5, 10.00m);
        order.Submit();
        order.Complete();
        order.Status.Should().Be(PurchaseStatus.Completed);
    }

    [Fact]
    public void Complete_Cancelled_Should_Throw()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(Guid.NewGuid(), "Product A", 5, 10.00m);
        order.Submit();
        order.Cancel();
        var act = () => order.Complete();
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void RecordInvoiceNumber_Should_Set_Value()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.RecordInvoiceNumber("INV-12345");
        order.SupplierInvoiceNumber.Should().Be("INV-12345");
    }

    [Fact]
    public void UpdateDetails_Should_Update_Supplier_And_Tax()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        var newSupplierId = Guid.NewGuid();
        order.UpdateDetails(newSupplierId, "Supplier B", "Notes", 15.00m);
        order.SupplierId.Should().Be(newSupplierId);
        order.SupplierName.Should().Be("Supplier B");
        order.TaxPercentage.Should().Be(15.00m);
    }

    [Fact]
    public void RemoveItem_Should_Decrease_Item_Count()
    {
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), "Supplier A");
        order.AddItem(productId, "Product A", 5, 10.00m);
        var itemId = order.Items.First().Id;
        order.RemoveItem(itemId);
        order.Items.Count.Should().Be(0);
    }
}
