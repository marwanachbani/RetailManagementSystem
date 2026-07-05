using FluentAssertions;
using RMS.Modules.Suppliers.Domain.Entities;
using RMS.Modules.Suppliers.Domain.Events;
using RMS.Modules.Suppliers.Domain.ValueObjects;
using Xunit;

namespace RMS.UnitTests.Suppliers;

public class SupplierTests
{
    [Fact]
    public void Create_With_Valid_Data_Should_Succeed()
    {
        var id = Guid.NewGuid();
        var supplier = Supplier.Create(
            id,
            "Acme Supplies",
            PhoneNumber.Create("+1234567890"),
            "John Smith",
            Email.Create("john@acme.com"),
            "VAT123456",
            Address.Create("123 Main St", "Springfield", "12345", "USA"));

        supplier.Should().NotBeNull();
        supplier.SupplierCode.Should().StartWith("SUP-");
        supplier.Status.Should().Be(SupplierStatus.Active);
        supplier.DomainEvents.Should().ContainSingle(e => e is SupplierCreatedEvent);
    }

    [Fact]
    public void Create_Without_CompanyName_Should_Fail()
    {
        var act = () => Supplier.Create(
            Guid.NewGuid(),
            "",
            PhoneNumber.Create("+1234567890"),
            null, null, null, null);

        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void Create_Without_PhoneNumber_Should_Fail()
    {
        var act = () => Supplier.Create(
            Guid.NewGuid(),
            "Acme Supplies",
            PhoneNumber.Create(""),
            null, null, null, null);

        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void Update_Should_Modify_Properties_And_Raise_Event()
    {
        var id = Guid.NewGuid();
        var supplier = Supplier.Create(
            id,
            "Acme Supplies",
            PhoneNumber.Create("+1234567890"),
            null, null, null, null);

        supplier.ClearDomainEvents();

        supplier.Update(
            "Acme Global",
            PhoneNumber.Create("+9876543210"),
            "Jane Doe",
            Email.Create("jane@acme.com"),
            "VAT987654",
            Address.Create("456 Oak St", "Shelbyville", "54321", "USA"));

        supplier.CompanyName.Should().Be("Acme Global");
        supplier.PhoneNumber.Value.Should().Be("+9876543210");
        supplier.ContactPerson.Should().Be("Jane Doe");
        supplier.DomainEvents.Should().ContainSingle(e => e is SupplierUpdatedEvent);
    }

    [Fact]
    public void Deactivate_Should_Set_Status_To_Inactive_And_Raise_Event()
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            "Acme Supplies",
            PhoneNumber.Create("+1234567890"),
            null, null, null, null);

        supplier.ClearDomainEvents();
        supplier.Deactivate();

        supplier.Status.Should().Be(SupplierStatus.Inactive);
        supplier.DomainEvents.Should().ContainSingle(e => e is SupplierDeactivatedEvent);
    }

    [Fact]
    public void Deactivate_Already_Inactive_Should_Throw()
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            "Acme Supplies",
            PhoneNumber.Create("+1234567890"),
            null, null, null, null);

        supplier.Deactivate();
        supplier.ClearDomainEvents();
        var act = () => supplier.Deactivate();

        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void Reactivate_Should_Set_Status_To_Active_And_Raise_Event()
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            "Acme Supplies",
            PhoneNumber.Create("+1234567890"),
            null, null, null, null);

        supplier.Deactivate();
        supplier.ClearDomainEvents();
        supplier.Reactivate();

        supplier.Status.Should().Be(SupplierStatus.Active);
        supplier.DomainEvents.Should().ContainSingle(e => e is SupplierReactivatedEvent);
    }

    [Fact]
    public void Reactivate_Already_Active_Should_Throw()
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            "Acme Supplies",
            PhoneNumber.Create("+1234567890"),
            null, null, null, null);

        supplier.ClearDomainEvents();
        var act = () => supplier.Reactivate();

        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void EnsureActiveForPurchaseOrder_When_Active_Should_Not_Throw()
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            "Acme Supplies",
            PhoneNumber.Create("+1234567890"),
            null, null, null, null);

        var act = () => supplier.EnsureActiveForPurchaseOrder();
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureActiveForPurchaseOrder_When_Inactive_Should_Throw()
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            "Acme Supplies",
            PhoneNumber.Create("+1234567890"),
            null, null, null, null);

        supplier.Deactivate();
        var act = () => supplier.EnsureActiveForPurchaseOrder();
        act.Should().Throw<RMS.BuildingBlocks.Exceptions.BusinessRuleValidationException>();
    }

    [Fact]
    public void SupplierCode_Should_Be_Generated()
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            "Acme Supplies",
            PhoneNumber.Create("+1234567890"),
            null, null, null, null);

        supplier.SupplierCode.Should().NotBeNullOrEmpty();
        supplier.SupplierCode.Should().StartWith("SUP-");
    }

    [Fact]
    public void Rehydrate_Should_Restore_State()
    {
        var id = Guid.NewGuid();
        var phone = PhoneNumber.Create("+1234567890");
        var address = Address.Create("123 Main St", "Springfield", "12345", "USA");
        var supplier = Supplier.Rehydrate(
            id,
            "SUP-TEST",
            "Acme Supplies",
            "John",
            phone,
            null,
            null,
            address,
            SupplierStatus.Active,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow);

        supplier.Id.Should().Be(id);
        supplier.SupplierCode.Should().Be("SUP-TEST");
        supplier.Address.Should().Be(address);
    }
}
