using FluentAssertions;
using RMS.Modules.Printing.Infrastructure.EscPos;
using Xunit;

namespace RMS.UnitTests.Printing.Infrastructure;

public class EscPosBuilderTests
{
    [Fact]
    public void Reset_Should_InitializeCommand()
    {
        var builder = new EscPosBuilder().Reset();
        var bytes = builder.Build();
        bytes.Should().NotBeEmpty();
        bytes[0].Should().Be(0x1B);
        bytes[1].Should().Be(0x40);
    }

    [Fact]
    public void AlignCenter_Should_AddAlignmentCommand()
    {
        var builder = new EscPosBuilder().Reset().Align(TextAlign.Center);
        var bytes = builder.Build();
        bytes.Should().Contain(new byte[] { 0x1B, 0x61, 0x01 });
    }

    [Fact]
    public void AlignRight_Should_AddAlignmentCommand()
    {
        var builder = new EscPosBuilder().Reset().Align(TextAlign.Right);
        var bytes = builder.Build();
        bytes.Should().Contain(new byte[] { 0x1B, 0x61, 0x02 });
    }

    [Fact]
    public void Bold_Should_AddBoldCommands()
    {
        var builder = new EscPosBuilder().Reset().Bold(true).Text("Test").Bold(false);
        var bytes = builder.Build();
        bytes.Should().Contain(new byte[] { 0x1B, 0x45, 0x01 });
        bytes.Should().Contain(new byte[] { 0x1B, 0x45, 0x00 });
    }

    [Fact]
    public void Text_Should_EncodeAscii()
    {
        var builder = new EscPosBuilder().Reset().Text("Hello");
        var bytes = builder.Build();
        bytes.Should().Contain(new byte[] { 0x1B, 0x40, (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o' });
    }

    [Fact]
    public void Line_Should_AddNewline()
    {
        var builder = new EscPosBuilder().Reset().Line("Test");
        var bytes = builder.Build();
        bytes.Should().Contain(new byte[] { 0x0A });
    }

    [Fact]
    public void Feed_Should_AddMultipleNewlines()
    {
        var builder = new EscPosBuilder().Reset().Feed(3);
        var bytes = builder.Build();
        var newlineCount = bytes.Count(b => b == 0x0A);
        newlineCount.Should().Be(3);
    }

    [Fact]
    public void Cut_Should_AddCutCommand()
    {
        var builder = new EscPosBuilder().Reset().Cut();
        var bytes = builder.Build();
        bytes.Should().Contain(new byte[] { 0x1D, 0x56, 0x00 });
    }

    [Fact]
    public void FeedAndCut_Should_AddFeedAndCut()
    {
        var builder = new EscPosBuilder().Reset().FeedAndCut(2);
        var bytes = builder.Build();
        var newlineCount = bytes.Count(b => b == 0x0A);
        newlineCount.Should().Be(2);
        bytes.Should().Contain(new byte[] { 0x1D, 0x56, 0x00 });
    }

    [Fact]
    public void OpenDrawer_Should_AddKickCommand()
    {
        var builder = new EscPosBuilder().Reset().OpenDrawer();
        var bytes = builder.Build();
        bytes.Should().Contain(new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA });
    }

    [Fact]
    public void RasterImage_Should_AddGsVCommand()
    {
        var matrix = new bool[10, 10];
        for (var y = 0; y < 10; y++)
            for (var x = 0; x < 10; x++)
                matrix[y, x] = (x + y) % 2 == 0;

        var builder = new EscPosBuilder().Reset().RasterImage(matrix);
        var bytes = builder.Build();
        bytes.Should().Contain(new byte[] { 0x1D, 0x76, 0x30, 0x00 });
    }

    [Fact]
    public void ComplexReceipt_Should_BuildSuccessfully()
    {
        var builder = new EscPosBuilder().Reset()
            .Align(TextAlign.Center)
            .Bold().Line("My Store").Bold(false)
            .Line("Address")
            .Line("Tel: 555-0100")
            .Line(new string('-', 32))
            .Align(TextAlign.Left)
            .Line("Receipt: R-001")
            .Line("Date: 2025-01-01")
            .Line("Item x1  10.00")
            .Line(new string('-', 32))
            .Line("TOTAL: 10.00")
            .Align(TextAlign.Center)
            .Line("Thank you!")
            .FeedAndCut(3);

        var bytes = builder.Build();
        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(100);
    }
}
