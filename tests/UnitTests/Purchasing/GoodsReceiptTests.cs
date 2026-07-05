using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Purchasing.Domain.Entities;
using Xunit;

namespace RMS.UnitTests.Purchasing;

public class GoodsReceiptTests
{
    [Fact]
    public void Create_With_Valid_Data_Should_Succeed()
    {
        var receipt = GoodsReceipt.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, "B001", DateTime.UtcNow.AddYears(1));
        receipt.QuantityReceived.Should().Be(10);
        receipt.BatchNumber.Should().Be("B001");
    }

    [Fact]
    public void Create_With_Zero_Quantity_Should_Throw()
    {
        var act = () => GoodsReceipt.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0);
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Rehydrate_Should_Preserve_All_Properties()
    {
        var id = Guid.NewGuid();
        var poId = Guid.NewGuid();
        var prodId = Guid.NewGuid();
        var receivedAt = DateTime.UtcNow;
        var expiry = DateTime.UtcNow.AddYears(1);
        var receipt = GoodsReceipt.Rehydrate(id, poId, prodId, 25, receivedAt, "B002", expiry);
        receipt.Id.Should().Be(id);
        receipt.PurchaseOrderId.Should().Be(poId);
        receipt.ProductId.Should().Be(prodId);
        receipt.QuantityReceived.Should().Be(25);
        receipt.ReceivedAt.Should().Be(receivedAt);
        receipt.BatchNumber.Should().Be("B002");
        receipt.ExpiryDate.Should().Be(expiry);
    }
}
