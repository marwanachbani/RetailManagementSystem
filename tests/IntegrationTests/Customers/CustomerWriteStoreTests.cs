using FluentAssertions;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Customers.Domain.Entities;
using RMS.Modules.Customers.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RMS.IntegrationTests.Customers;

public class CustomerWriteStoreTests : CustomerIntegrationTestBase, IClassFixture<CustomerTestDatabaseFixture>
{
    public CustomerWriteStoreTests(CustomerTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task InsertAsync_Should_PersistCustomer()
    {
        var customer = CreateSampleCustomer();

        await WriteStore.InsertAsync(customer);

        var result = await ReadStore.GetByIdAsync(customer.Id);
        result.Should().NotBeNull();
        result!.CustomerCode.Should().Be(customer.CustomerCode);
        result.FullName.Should().Be(customer.FullName);
    }

    [Fact]
    public async Task InsertAsync_Should_PersistDomainEvents()
    {
        var customer = CreateSampleCustomer();
        var eventStore = Fixture.Services.GetRequiredService<IEventStore>();

        await WriteStore.InsertAsync(customer);

        var events = await eventStore.GetByAggregateIdAsync(customer.Id);
        events.Should().Contain(e => e.EventType.Contains("CustomerCreatedEvent"));
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyCustomer()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);
        customer.ClearDomainEvents();

        var newPhone = PhoneNumber.Create("+15559998888");
        var newEmail = Email.Create("updated@example.com");
        var newAddress = Address.Create("999 New St", "Chicago", "60601", "USA");
        customer.Update("Updated", "Name", newPhone, newEmail, newAddress);

        await WriteStore.UpdateAsync(customer);

        var result = await ReadStore.GetByIdAsync(customer.Id);
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateAsync_Should_PersistDomainEvents()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);
        customer.ClearDomainEvents();

        customer.Deactivate();
        var eventStore = Fixture.Services.GetRequiredService<IEventStore>();

        await WriteStore.UpdateAsync(customer);

        var events = await eventStore.GetByAggregateIdAsync(customer.Id);
        events.Should().Contain(e => e.EventType.Contains("CustomerDeactivatedEvent"));
    }

    [Fact]
    public async Task InsertAsync_WithNullEmailAndAddress_Should_PersistCustomer()
    {
        var phone = PhoneNumber.Create("+15551234567");
        var customer = Customer.Create(Guid.NewGuid(), "Minimal", "Customer", phone);

        await WriteStore.InsertAsync(customer);

        var result = await ReadStore.GetByIdAsync(customer.Id);
        result.Should().NotBeNull();
        result!.Email.Should().BeNull();
        result.Street.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_DeactivateAndReactivate_Should_PersistStatusChanges()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);
        customer.ClearDomainEvents();

        customer.Deactivate();
        await WriteStore.UpdateAsync(customer);

        var inactive = await ReadStore.GetByIdAsync(customer.Id);
        inactive!.Status.Should().Be("Inactive");

        customer.Reactivate();
        await WriteStore.UpdateAsync(customer);

        var reactivated = await ReadStore.GetByIdAsync(customer.Id);
        reactivated!.Status.Should().Be("Active");
    }
}
