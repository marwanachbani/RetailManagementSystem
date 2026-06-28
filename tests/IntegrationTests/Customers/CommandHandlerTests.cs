using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RMS.BuildingBlocks.Results;
using RMS.Modules.Customers.Application.CreateCustomer;
using RMS.Modules.Customers.Application.DeactivateCustomer;
using RMS.Modules.Customers.Application.GetCustomerById;
using RMS.Modules.Customers.Application.GetCustomersPaged;
using RMS.Modules.Customers.Application.ReactivateCustomer;
using RMS.Modules.Customers.Application.UpdateCustomer;
using RMS.Modules.Customers.Domain.Entities;
using Xunit;

namespace RMS.IntegrationTests.Customers;

public class CustomerCommandHandlerTests : CustomerIntegrationTestBase, IClassFixture<CustomerTestDatabaseFixture>
{
    private readonly IMediator _mediator;

    public CustomerCommandHandlerTests(CustomerTestDatabaseFixture fixture) : base(fixture)
    {
        _mediator = fixture.Services.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task CreateCustomer_Should_ReturnCustomerId()
    {
        var command = new CreateCustomerCommand("Jane", "Smith", "+15551234567", "jane@example.com", "456 Oak Ave", "Boston", "02101", "USA");
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicatePhoneNumber_Should_Fail()
    {
        var customer = CreateSampleCustomer(phone: "+15551234567");
        await WriteStore.InsertAsync(customer);

        var command = new CreateCustomerCommand("Another", "Person", "+15551234567", "another@example.com", "123 St", "City", "00000", "USA");
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Customers.DuplicatePhoneNumber");
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_Should_Fail()
    {
        var customer = CreateSampleCustomer(email: "duplicate@example.com");
        await WriteStore.InsertAsync(customer);

        var command = new CreateCustomerCommand("Another", "Person", "+15559876543", "duplicate@example.com", "123 St", "City", "00000", "USA");
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Customers.DuplicateEmail");
    }

    [Fact]
    public async Task UpdateCustomer_Should_UpdateCustomer()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);

        var command = new UpdateCustomerCommand(customer.Id, "Updated", "Name", "+15551112222", "updated@example.com", "999 New St", "Chicago", "60601", "USA");
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(customer.Id);
        updated!.FirstName.Should().Be("Updated");
        updated.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateCustomer_WithNonExistentId_Should_Fail()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Test", "User", "+15551112222", null, null, null, null, null);
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Customers.NotFound");
    }

    [Fact]
    public async Task DeactivateCustomer_Should_DeactivateCustomer()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);

        var command = new DeactivateCustomerCommand(customer.Id);
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(customer.Id);
        updated!.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task DeactivateCustomer_AlreadyInactive_Should_Fail()
    {
        var customer = CreateSampleCustomer();
        customer.Deactivate();
        await WriteStore.UpdateAsync(customer);

        var command = new DeactivateCustomerCommand(customer.Id);
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Customer.AlreadyInactive");
    }

    [Fact]
    public async Task ReactivateCustomer_Should_ReactivateCustomer()
    {
        var customer = CreateSampleCustomer();
        customer.Deactivate();
        await WriteStore.UpdateAsync(customer);

        var command = new ReactivateCustomerCommand(customer.Id);
        var result = await _mediator.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await ReadStore.GetByIdAsync(customer.Id);
        updated!.Status.Should().Be("Active");
    }

    [Fact]
    public async Task ReactivateCustomer_AlreadyActive_Should_Fail()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);

        var command = new ReactivateCustomerCommand(customer.Id);
        var result = await _mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Customer.AlreadyActive");
    }

    [Fact]
    public async Task GetCustomerById_Should_ReturnCustomer()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);

        var query = new GetCustomerByIdQuery(customer.Id);
        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(customer.Id);
        result.Value.FullName.Should().Be(customer.FullName);
    }

    [Fact]
    public async Task GetCustomerById_WithNonExistentId_Should_Fail()
    {
        var query = new GetCustomerByIdQuery(Guid.NewGuid());
        var result = await _mediator.Send(query);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("Customers.NotFound");
    }

    [Fact]
    public async Task GetCustomersPaged_Should_ReturnPagedResult()
    {
        for (int i = 0; i < 10; i++)
        {
            var customer = CreateSampleCustomer(id: Guid.NewGuid(), firstName: $"User{i}", phone: $"+1555000000{i:00}");
            await WriteStore.InsertAsync(customer);
        }

        var query = new GetCustomersPagedQuery(1, 5, null, true);
        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().BeGreaterThanOrEqualTo(10);
    }

    [Fact]
    public async Task GetCustomersPaged_WithSearchTerm_Should_FilterResults()
    {
        for (int i = 0; i < 5; i++)
        {
            var customer = CreateSampleCustomer(id: Guid.NewGuid(), firstName: $"Unique{i}", lastName: "Smith", phone: $"+1555000000{i:00}");
            await WriteStore.InsertAsync(customer);
        }

        var query = new GetCustomersPagedQuery(1, 10, "Unique", true);
        var result = await _mediator.Send(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().AllSatisfy(c => c.FirstName.Should().StartWith("Unique"));
    }
}
