using FluentAssertions;
using RMS.BuildingBlocks.Results;
using Xunit;

namespace RMS.UnitTests.BuildingBlocks;

public class ResultTests
{
    [Fact]
    public void Success_Result_Should_Have_No_Error()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_Result_Should_Carry_Error_Message()
    {
        var result = Result.Failure("Stock cannot be negative.", "STOCK_NEGATIVE");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Stock cannot be negative.");
        result.ErrorCode.Should().Be("STOCK_NEGATIVE");
    }

    [Fact]
    public void Generic_Success_Result_Should_Expose_Value()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Accessing_Value_On_Failure_Should_Throw()
    {
        var result = Result.Failure<int>("not found");

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }
}
