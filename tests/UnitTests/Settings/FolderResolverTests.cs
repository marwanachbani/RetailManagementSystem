using FluentAssertions;
using RMS.Modules.Settings.Application.Services;
using Xunit;

namespace RMS.UnitTests.Settings;

public class FolderResolverTests
{
    [Fact]
    public void Resolve_WithRootedValue_Should_ReturnItVerbatim()
    {
        var resolver = new FolderResolver(@"C:\RMS");
        resolver.Resolve(@"D:\Elsewhere", "Reports").Should().Be(@"D:\Elsewhere");
    }

    [Fact]
    public void Resolve_WithRelativeValue_Should_CombineWithBase()
    {
        var resolver = new FolderResolver(@"C:\RMS");
        resolver.Resolve("Reports", "Reports").Should().Be(Path.Combine(@"C:\RMS", "Reports"));
    }

    [Fact]
    public void Resolve_WithEmptyValue_Should_UseDefaultSubPath()
    {
        var resolver = new FolderResolver(@"C:\RMS");
        resolver.Resolve("", "Receipts").Should().Be(Path.Combine(@"C:\RMS", "Receipts"));
    }

    [Fact]
    public void GetDefaultPath_Should_CombineBaseAndSubPath()
    {
        var resolver = new FolderResolver(@"C:\RMS");
        resolver.GetDefaultPath("Backups").Should().Be(Path.Combine(@"C:\RMS", "Backups"));
    }

    [Fact]
    public void EnsureExists_Should_CreateMissingDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"rms_folder_{Guid.NewGuid():N}");
        try
        {
            var resolver = new FolderResolver(dir);
            var target = Path.Combine(dir, "Reports");
            resolver.EnsureExists(target);
            Directory.Exists(target).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }
}
