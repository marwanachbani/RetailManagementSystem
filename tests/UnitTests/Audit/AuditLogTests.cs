using FluentAssertions;
using RMS.Modules.Audit.Domain.Entities;
using Xunit;

namespace RMS.UnitTests.Audit;

public class AuditLogTests
{
    [Fact]
    public void AuditLog_Should_Create_With_All_Properties()
    {
        var auditLog = new AuditLog(
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

        auditLog.AuditId.Should().NotBeEmpty();
        auditLog.Module.Should().Be("Sales");
        auditLog.Action.Should().Be("Sale Created");
        auditLog.Entity.Should().Be("Sale");
        auditLog.OldValue.Should().BeNull();
        auditLog.NewValue.Should().Be("Sale123");
    }

    [Fact]
    public void AuditLog_Should_Allow_Nullable_Properties()
    {
        var auditLog = new AuditLog(
            Guid.NewGuid(),
            DateTime.UtcNow,
            null,
            "System",
            "Inventory",
            "Stock Changed",
            "InventoryItem",
            null,
            "10",
            "20",
            "Machine01",
            "1.0.0.0");

        auditLog.UserId.Should().BeNull();
        auditLog.EntityId.Should().BeNull();
        auditLog.OldValue.Should().Be("10");
        auditLog.NewValue.Should().Be("20");
    }
}
