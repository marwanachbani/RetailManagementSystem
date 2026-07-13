using FluentAssertions;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Infrastructure.Barcode;
using Xunit;

namespace RMS.UnitTests.Printing.Infrastructure;

public class BarcodeGeneratorTests
{
    private readonly IBarcodeGenerator _generator = new BarcodeGenerator();

    [Fact]
    public void Generate_Code128_Should_ReturnNonEmptyPng()
    {
        var png = _generator.Generate("TEST-123", BarcodeSymbology.Code128, 200, 80);
        png.Should().NotBeEmpty();
        png.Length.Should().BeGreaterThan(100);
        png[0].Should().Be(0x89); // PNG signature
        png[1].Should().Be(0x50); // 'P'
    }

    [Fact]
    public void Generate_Code39_Should_ReturnValidPng()
    {
        var png = _generator.Generate("ABC123", BarcodeSymbology.Code39, 200, 80);
        png.Should().NotBeEmpty();
        png[0].Should().Be(0x89);
    }

    [Fact]
    public void Generate_EAN13_Should_ReturnValidPng()
    {
        var png = _generator.Generate("5901234123457", BarcodeSymbology.EAN13, 200, 80);
        png.Should().NotBeEmpty();
        png[0].Should().Be(0x89);
    }

    [Fact]
    public void Generate_QRCode_Should_ReturnValidPng()
    {
        var png = _generator.Generate("https://rms.local", BarcodeSymbology.QRCode, 200, 200);
        png.Should().NotBeEmpty();
        png[0].Should().Be(0x89);
    }

    [Fact]
    public void GenerateQr_Should_ReturnValidPng()
    {
        var png = _generator.GenerateQr("https://rms.local", 150);
        png.Should().NotBeEmpty();
        png[0].Should().Be(0x89);
    }

    [Fact]
    public void Generate_EmptyContent_Should_Throw()
    {
        Action act = () => _generator.Generate("", BarcodeSymbology.Code128, 200, 80);
        act.Should().Throw<RMS.Modules.Printing.Domain.PrintingException>();
    }
}
