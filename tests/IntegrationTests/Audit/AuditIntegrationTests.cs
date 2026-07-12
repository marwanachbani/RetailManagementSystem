using FluentAssertions;
using RMS.Modules.Audit.Domain.Entities;
using Xunit;

namespace RMS.IntegrationTests.Audit;

public class AuditIntegrationTests : AuditIntegrationTestBase, IClassFixture<AuditTestDatabaseFixture>
{
    public AuditIntegrationTests(AuditTestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task InsertAsync_Should_Persist_Audit_Log()
    {
        var entry = new AuditLog(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            "testuser",
            "Sales",
            "Sale Created",
            "Sale",
            Guid.NewGuid().ToString(),
            null,
            "Sale123",
            "Machine01",
            "1.0.0.0");

        await WriteStore.InsertAsync(entry);

        var result = await ReadStore.GetByIdAsync(entry.AuditId);
        result.Should().NotBeNull();
        result!.UserName.Should().Be("testuser");
        result.Module.Should().Be("Sales");
        result.Action.Should().Be("Sale Created");
    }

    [Fact]
    public async Task GetPagedAsync_Should_Return_Paged_Results()
    {
        for (int i = 0; i < 5; i++)
        {
            var entry = new AuditLog(
                Guid.NewGuid(),
                DateTime.UtcNow.AddMinutes(-i),
                Guid.NewGuid(),
                $"user{i}",
                "Products",
                "Created",
                "Product",
                Guid.NewGuid().ToString(),
                null,
                $"Product{i}",
                "Machine01",
                "1.0.0.0");
            await WriteStore.InsertAsync(entry);
        }

        var result = await ReadStore.GetPagedAsync(1, 2, null, null, null, null, null, null);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Filter_By_Module()
    {
        await WriteStore.InsertAsync(new AuditLog(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "u1", "Sales", "Sale Created", "Sale", "1", null, "S1", "M1", "1.0"));
        await WriteStore.InsertAsync(new AuditLog(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "u2", "Products", "Created", "Product", "1", null, "P1", "M1", "1.0"));

        var result = await ReadStore.GetPagedAsync(1, 10, null, null, null, "Sales", null, null);

        result.Items.Should().ContainSingle();
        result.Items[0].Module.Should().Be("Sales");
    }
}
