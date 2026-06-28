using FluentAssertions;
using RMS.Modules.Customers.Domain.Entities;
using Xunit;

namespace RMS.IntegrationTests.Customers;

public class CustomerReadStoreTests : CustomerIntegrationTestBase, IClassFixture<CustomerTestDatabaseFixture>
{
    public CustomerReadStoreTests(CustomerTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_Should_ReturnCustomer()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);

        var result = await ReadStore.GetByIdAsync(customer.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.CustomerCode.Should().Be(customer.CustomerCode);
        result.FullName.Should().Be(customer.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerNotExists_Should_ReturnNull()
    {
        var result = await ReadStore.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPhoneNumberAsync_Should_ReturnCustomer()
    {
        var customer = CreateSampleCustomer(phone: "+15551234567");
        await WriteStore.InsertAsync(customer);

        var result = await ReadStore.GetByPhoneNumberAsync("+15551234567");

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_Should_ReturnCustomer()
    {
        var customer = CreateSampleCustomer(email: "test@example.com");
        await WriteStore.InsertAsync(customer);

        var result = await ReadStore.GetByEmailAsync("test@example.com");

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetByCustomerCodeAsync_Should_ReturnCustomer()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);

        var result = await ReadStore.GetByCustomerCodeAsync(customer.CustomerCode);

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task SearchAsync_Should_ReturnMatchingCustomers()
    {
        for (int i = 0; i < 5; i++)
        {
            var customer = CreateSampleCustomer(id: Guid.NewGuid(), firstName: $"Searchable{i}", phone: $"+1555000000{i:00}");
            await WriteStore.InsertAsync(customer);
        }

        var results = await ReadStore.SearchAsync("Searchable", true);

        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPagedAsync_Should_ReturnPagedResult()
    {
        for (int i = 0; i < 7; i++)
        {
            var customer = CreateSampleCustomer(id: Guid.NewGuid(), phone: $"+1555000000{i:00}");
            await WriteStore.InsertAsync(customer);
        }

        var page = await ReadStore.GetPagedAsync(1, 3, null, true);

        page.Items.Should().HaveCount(3);
        page.PageNumber.Should().Be(1);
        page.PageSize.Should().Be(3);
        page.TotalCount.Should().Be(7);
    }

    [Fact]
    public async Task GetPagedAsync_ExcludingInactive_Should_FilterOutInactive()
    {
        var activeCustomer = CreateSampleCustomer(id: Guid.NewGuid(), firstName: "Active");
        await WriteStore.InsertAsync(activeCustomer);

        var inactiveCustomer = CreateSampleCustomer(id: Guid.NewGuid(), firstName: "Inactive");
        await WriteStore.InsertAsync(inactiveCustomer);
        inactiveCustomer.Deactivate();
        await WriteStore.UpdateAsync(inactiveCustomer);

        var page = await ReadStore.GetPagedAsync(1, 10, null, false);

        page.Items.Should().ContainSingle(c => c.FirstName == "Active");
    }

    [Fact]
    public async Task HasSalesAsync_WhenNoSales_Should_ReturnFalse()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);

        var result = await ReadStore.HasSalesAsync(customer.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatisticsAsync_WhenCustomerNotExists_Should_ReturnNull()
    {
        var result = await ReadStore.GetStatisticsAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPurchaseHistoryAsync_WhenNoSales_Should_ReturnEmptyList()
    {
        var customer = CreateSampleCustomer();
        await WriteStore.InsertAsync(customer);

        var history = await ReadStore.GetPurchaseHistoryAsync(customer.Id);

        history.Should().BeEmpty();
    }
}
