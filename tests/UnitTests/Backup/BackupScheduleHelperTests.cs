using FluentAssertions;
using RMS.Modules.Backup.Application;
using Xunit;

namespace RMS.UnitTests.Backup;

public class BackupScheduleHelperTests
{
    [Fact]
    public void IsDue_WithNoPreviousBackup_Should_BeFalseBeforeTime()
    {
        var now = new DateTime(2026, 7, 12, 10, 0, 0);
        var result = BackupScheduleHelper.IsDue(null, "Daily", new TimeSpan(23, 0, 0), now);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDue_WithNoPreviousBackup_Should_BeTrueWhenOverdue()
    {
        var now = new DateTime(2026, 7, 12, 23, 5, 0);
        var result = BackupScheduleHelper.IsDue(null, "Daily", new TimeSpan(23, 0, 0), now);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDue_WithDailyFrequency_Should_BeTrueWhenOverdue()
    {
        var last = new DateTime(2026, 7, 12, 10, 0, 0);
        var now = new DateTime(2026, 7, 13, 10, 5, 0);
        var result = BackupScheduleHelper.IsDue(last, "Daily", new TimeSpan(10, 0, 0), now);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDue_WithWeeklyFrequency_Should_BeTrueWhenOverdue()
    {
        var last = new DateTime(2026, 7, 12, 10, 0, 0);
        var now = new DateTime(2026, 7, 26, 10, 1, 0);
        var result = BackupScheduleHelper.IsDue(last, "Weekly", new TimeSpan(10, 0, 0), now);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDue_WithMonthlyFrequency_Should_BeTrueWhenOverdue()
    {
        var last = new DateTime(2026, 7, 12, 10, 0, 0);
        var now = new DateTime(2026, 9, 12, 10, 1, 0);
        var result = BackupScheduleHelper.IsDue(last, "Monthly", new TimeSpan(10, 0, 0), now);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDue_Should_BeFalseBeforeInterval()
    {
        var last = new DateTime(2026, 7, 12, 10, 0, 0);
        var now = new DateTime(2026, 7, 12, 10, 5, 0);
        var result = BackupScheduleHelper.IsDue(last, "Daily", new TimeSpan(10, 0, 0), now);
        result.Should().BeFalse();
    }

    [Fact]
    public void ComputeNextRun_Daily_Should_ReturnNextDayAtTime()
    {
        var last = new DateTime(2026, 7, 12, 10, 0, 0);
        var now = new DateTime(2026, 7, 12, 11, 0, 0);
        var next = BackupScheduleHelper.ComputeNextRun(last, "Daily", new TimeSpan(10, 0, 0), now);
        next.Should().Be(new DateTime(2026, 7, 13, 10, 0, 0));
    }

    [Fact]
    public void ComputeNextRun_Weekly_Should_Return7DaysLaterAtTime()
    {
        var last = new DateTime(2026, 7, 12, 10, 0, 0);
        var now = new DateTime(2026, 7, 12, 11, 0, 0);
        var next = BackupScheduleHelper.ComputeNextRun(last, "Weekly", new TimeSpan(10, 0, 0), now);
        next.Should().Be(new DateTime(2026, 7, 19, 10, 0, 0));
    }

    [Fact]
    public void ComputeNextRun_Monthly_Should_Return1MonthLaterAtTime()
    {
        var last = new DateTime(2026, 7, 12, 10, 0, 0);
        var now = new DateTime(2026, 7, 12, 11, 0, 0);
        var next = BackupScheduleHelper.ComputeNextRun(last, "Monthly", new TimeSpan(10, 0, 0), now);
        next.Should().Be(new DateTime(2026, 8, 12, 10, 0, 0));
    }

    [Fact]
    public void ComputeNextRun_NoPrevious_Should_ReturnTodayAtTime()
    {
        var now = new DateTime(2026, 7, 12, 10, 0, 0);
        var next = BackupScheduleHelper.ComputeNextRun(null, "Daily", new TimeSpan(23, 0, 0), now);
        next.Should().Be(new DateTime(2026, 7, 12, 23, 0, 0));
    }

    [Fact]
    public void ComputeNextRun_NoPreviousOverdue_Should_ReturnNow()
    {
        var now = new DateTime(2026, 7, 12, 23, 5, 0);
        var next = BackupScheduleHelper.ComputeNextRun(null, "Daily", new TimeSpan(23, 0, 0), now);
        next.Should().Be(now);
    }

    [Fact]
    public void ComputeNextRun_Should_AdvanceOverdueToNextInterval()
    {
        var last = new DateTime(2026, 7, 12, 14, 0, 0);
        var now = new DateTime(2026, 7, 12, 14, 30, 0);
        var next = BackupScheduleHelper.ComputeNextRun(last, "Daily", new TimeSpan(14, 0, 0), now);
        next.Should().Be(new DateTime(2026, 7, 13, 14, 0, 0));
    }

    [Theory]
    [InlineData("weekly")]
    [InlineData("WEEKLY")]
    [InlineData("Weekly")]
    public void ComputeNextRun_Should_BeCaseInsensitiveForFrequency(string frequency)
    {
        var last = new DateTime(2026, 7, 12, 10, 0, 0);
        var now = new DateTime(2026, 7, 12, 11, 0, 0);
        var next = BackupScheduleHelper.ComputeNextRun(last, frequency, new TimeSpan(10, 0, 0), now);
        next.Should().Be(new DateTime(2026, 7, 19, 10, 0, 0));
    }
}
