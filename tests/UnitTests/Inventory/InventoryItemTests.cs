using FluentAssertions;
using RMS.BuildingBlocks.Exceptions;
using RMS.Modules.Inventory.Domain.Entities;
using RMS.Modules.Inventory.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Inventory;

public class InventoryItemTests
{
    [Fact]
    public void Create_Should_InitializeWithZeroQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 0, 10);

        item.CurrentQuantity.Value.Should().Be(0);
        item.IsActive.Should().BeTrue();
        item.LowStockThreshold.Should().Be(10);
    }

    [Fact]
    public void Create_WithInitialQuantity_Should_SetQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 50, 10);

        item.CurrentQuantity.Value.Should().Be(50);
    }

    [Fact]
    public void Create_WithEmptyProductId_Should_Throw()
    {
        Action act = () => InventoryItem.Create(Guid.NewGuid(), Guid.Empty, 0, 10);

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Inventory.ProductIdRequired");
    }

    [Fact]
    public void Create_Should_RaiseInventoryCreatedEvent()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 0, 10);

        item.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "InventoryCreatedEvent");
    }

    [Fact]
    public void Create_WhenBelowThreshold_Should_RaiseLowStockDetectedEvent()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 5, 10);

        item.DomainEvents.Should().Contain(e => e.GetType().Name == "LowStockDetectedEvent");
    }

    [Fact]
    public void IncreaseStock_Should_AddQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.ClearDomainEvents();

        item.IncreaseStock(5, "Restock");

        item.CurrentQuantity.Value.Should().Be(15);
    }

    [Fact]
    public void IncreaseStock_Should_RaiseStockIncreasedEvent()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.ClearDomainEvents();

        item.IncreaseStock(5, "Restock");

        item.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "StockIncreasedEvent");
    }

    [Fact]
    public void IncreaseStock_WithZeroAmount_Should_Throw()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);

        Action act = () => item.IncreaseStock(0, "Invalid");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Inventory.InvalidAmount");
    }

    [Fact]
    public void IncreaseStock_WithoutReason_Should_Throw()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);

        Action act = () => item.IncreaseStock(5, "");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Inventory.ReasonRequired");
    }

    [Fact]
    public void DecreaseStock_Should_SubtractQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 20, 10);
        item.ClearDomainEvents();

        item.DecreaseStock(5, "Sale");

        item.CurrentQuantity.Value.Should().Be(15);
    }

    [Fact]
    public void DecreaseStock_Should_RaiseStockDecreasedEvent()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 20, 10);
        item.ClearDomainEvents();

        item.DecreaseStock(5, "Sale");

        item.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "StockDecreasedEvent");
    }

    [Fact]
    public void DecreaseStock_BelowZero_Should_Throw()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 5, 10);

        Action act = () => item.DecreaseStock(10, "Sale");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "StockQuantity.InsufficientStock");
    }

    [Fact]
    public void AdjustStock_Should_SetNewQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.ClearDomainEvents();

        item.AdjustStock(25, "Inventory count");

        item.CurrentQuantity.Value.Should().Be(25);
    }

    [Fact]
    public void AdjustStock_Should_RaiseStockAdjustedEvent()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.ClearDomainEvents();

        item.AdjustStock(25, "Inventory count");

        item.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "StockAdjustedEvent");
    }

    [Fact]
    public void Deactivate_Should_SetIsActiveToFalse()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.ClearDomainEvents();

        item.Deactivate();

        item.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_Should_NotThrow()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.Deactivate();
        item.ClearDomainEvents();

        item.Deactivate();

        item.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IncreaseStock_WhenInactive_Should_Throw()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.Deactivate();
        item.ClearDomainEvents();

        Action act = () => item.IncreaseStock(5, "Restock");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Inventory.Inactive");
    }

    [Fact]
    public void DecreaseStock_WhenInactive_Should_Throw()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.Deactivate();
        item.ClearDomainEvents();

        Action act = () => item.DecreaseStock(5, "Sale");

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.RuleName == "Inventory.Inactive");
    }

    [Fact]
    public void UpdateLowStockThreshold_Should_UpdateThreshold()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.ClearDomainEvents();

        item.UpdateLowStockThreshold(5);

        item.LowStockThreshold.Should().Be(5);
    }

    [Fact]
    public void UpdateLowStockThreshold_WhenBelowNewThreshold_Should_RaiseLowStockEvent()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);
        item.ClearDomainEvents();

        item.UpdateLowStockThreshold(15);

        item.DomainEvents.Should().Contain(e => e.GetType().Name == "LowStockDetectedEvent");
    }

    [Fact]
    public void Transactions_Should_BeRecordedOnStockChange()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 10);

        item.IncreaseStock(5, "Restock");

        item.Transactions.Should().ContainSingle();
        item.Transactions.First().ChangeAmount.Should().Be(5);
        item.Transactions.First().Reason.Should().Be("Restock");
    }
}
