using FluentAssertions;
using RMS.Modules.Notifications.Domain;
using Xunit;

namespace RMS.UnitTests.Notifications;

public class NotificationTests
{
    [Fact]
    public void Create_Should_InitializeDefaults()
    {
        var notification = new Notification(
            Guid.NewGuid(),
            "Test Title",
            "Test Message",
            NotificationCategory.Sales,
            NotificationSeverity.Information,
            "Sales");

        notification.IsRead.Should().BeFalse();
        notification.ReadOn.Should().BeNull();
        notification.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        notification.UserId.Should().BeNull();
        notification.RelatedEntityId.Should().BeNull();
    }

    [Fact]
    public void MarkAsRead_Should_SetReadProperties()
    {
        var notification = new Notification(
            Guid.NewGuid(),
            "Test Title",
            "Test Message",
            NotificationCategory.Sales,
            NotificationSeverity.Information,
            "Sales");

        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
        notification.ReadOn.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsRead_ShouldNot_Change_WhenAlreadyRead()
    {
        var notification = new Notification(
            Guid.NewGuid(),
            "Test Title",
            "Test Message",
            NotificationCategory.Sales,
            NotificationSeverity.Information,
            "Sales");

        notification.MarkAsRead();
        var firstReadOn = notification.ReadOn;

        notification.MarkAsRead();
        notification.ReadOn.Should().Be(firstReadOn);
    }
}
