using FluentAssertions;
using RMS.Modules.Customers.Domain.Entities;
using RMS.Modules.Customers.Domain.ValueObjects;
using RMS.BuildingBlocks.Exceptions;
using Xunit;

namespace RMS.UnitTests.Customers;

public class CustomerTests
{
    private static Customer CreateSampleCustomer(
        string firstName = "John",
        string lastName = "Doe",
        string phone = "+1234567890",
        string? email = "john@example.com",
        string? street = "123 Main St",
        string? city = "New York")
    {
        var phoneNumber = PhoneNumber.Create(phone);
        var emailObj = email is not null ? Email.Create(email) : null;
        var address = Address.Create(street!, city!, "10001", "USA");
        return Customer.Create(Guid.NewGuid(), firstName, lastName, phoneNumber, emailObj, address);
    }

    [Fact]
    public void Create_WithValidData_Should_CreateActiveCustomer()
    {
        var customer = CreateSampleCustomer();

        customer.CustomerCode.Should().StartWith("CUST-");
        customer.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.FullName.Should().Be("John Doe");
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "CustomerCreatedEvent");
    }

    [Fact]
    public void Create_WithEmptyFirstName_Should_Throw()
    {
        var act = () => Customer.Create(Guid.NewGuid(), "", "Doe", PhoneNumber.Create("+1234567890"));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.ErrorCode == "Customer.FirstNameEmpty");
    }

    [Fact]
    public void Create_WithEmptyLastName_Should_Throw()
    {
        var act = () => Customer.Create(Guid.NewGuid(), "John", "", PhoneNumber.Create("+1234567890"));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.ErrorCode == "Customer.LastNameEmpty");
    }

    [Fact]
    public void Create_WithTooLongFirstName_Should_Throw()
    {
        var act = () => Customer.Create(Guid.NewGuid(), new string('A', 101), "Doe", PhoneNumber.Create("+1234567890"));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.ErrorCode == "Customer.FirstNameTooLong");
    }

    [Fact]
    public void Update_WithValidData_Should_UpdateCustomer()
    {
        var customer = CreateSampleCustomer();
        customer.ClearDomainEvents();

        var newPhone = PhoneNumber.Create("+9876543210");
        var newEmail = Email.Create("updated@example.com");
        var newAddress = Address.Create("456 Oak Ave", "Boston", "02101", "USA");

        customer.Update("Jane", "Smith", newPhone, newEmail, newAddress);

        customer.FirstName.Should().Be("Jane");
        customer.LastName.Should().Be("Smith");
        customer.FullName.Should().Be("Jane Smith");
        customer.PhoneNumber.Should().Be(newPhone);
        customer.Email.Should().Be(newEmail);
        customer.Address.Should().Be(newAddress);
        customer.UpdatedAt.Should().NotBeNull();
        customer.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "CustomerUpdatedEvent");
    }

    [Fact]
    public void Deactivate_WhenActive_Should_SetInactive()
    {
        var customer = CreateSampleCustomer();
        customer.ClearDomainEvents();

        customer.Deactivate();

        customer.Status.Should().Be(CustomerStatus.Inactive);
        customer.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "CustomerDeactivatedEvent");
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_Should_Throw()
    {
        var customer = CreateSampleCustomer();
        customer.Deactivate();

        var act = () => customer.Deactivate();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.ErrorCode == "Customer.AlreadyInactive");
    }

    [Fact]
    public void Reactivate_WhenInactive_Should_SetActive()
    {
        var customer = CreateSampleCustomer();
        customer.Deactivate();
        customer.ClearDomainEvents();

        customer.Reactivate();

        customer.Status.Should().Be(CustomerStatus.Active);
        customer.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "CustomerReactivatedEvent");
    }

    [Fact]
    public void Reactivate_WhenAlreadyActive_Should_Throw()
    {
        var customer = CreateSampleCustomer();

        var act = () => customer.Reactivate();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.ErrorCode == "Customer.AlreadyActive");
    }

    [Fact]
    public void EnsureActiveForNewSale_WhenInactive_Should_Throw()
    {
        var customer = CreateSampleCustomer();
        customer.Deactivate();

        var act = () => customer.EnsureActiveForNewSale();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(e => e.ErrorCode == "Customer.InactiveForSale");
    }

    [Fact]
    public void EnsureActiveForNewSale_WhenActive_Should_NotThrow()
    {
        var customer = CreateSampleCustomer();

        var act = () => customer.EnsureActiveForNewSale();

        act.Should().NotThrow();
    }

    [Fact]
    public void Rehydrate_Should_RestoreAllProperties()
    {
        var id = Guid.NewGuid();
        var phone = PhoneNumber.Create("+1234567890");
        var email = Email.Create("test@example.com");
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        var customer = Customer.Rehydrate(id, "CUST-001", "John", "Doe", phone, email, address, CustomerStatus.Active, createdAt, updatedAt);

        customer.Id.Should().Be(id);
        customer.CustomerCode.Should().Be("CUST-001");
        customer.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.PhoneNumber.Should().Be(phone);
        customer.Email.Should().Be(email);
        customer.Address.Should().Be(address);
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.CreatedAt.Should().Be(createdAt);
        customer.UpdatedAt.Should().Be(updatedAt);
    }
}
