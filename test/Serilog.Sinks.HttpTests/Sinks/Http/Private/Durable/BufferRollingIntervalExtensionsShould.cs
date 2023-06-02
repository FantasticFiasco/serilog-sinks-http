using System;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable;

public class BufferRollingIntervalExtensionsShould
{
    [Theory]
    [InlineData(BufferRollingInterval.Year, RollingInterval.Year)]
    [InlineData(BufferRollingInterval.Month, RollingInterval.Month)]
    [InlineData(BufferRollingInterval.Day, RollingInterval.Day)]
    [InlineData(BufferRollingInterval.Hour, RollingInterval.Hour)]
    [InlineData(BufferRollingInterval.Minute, RollingInterval.Minute)]
    public void ConvertInterval(BufferRollingInterval input, RollingInterval want)
    {
        // Act
        var got = input.ToRollingInterval();

        // Assert
        got.ShouldBe(want);
    }

    [Fact]
    public void SupportAllIntervals()
    {
        // Arrange
        foreach (var input in Enum.GetValues<BufferRollingInterval>())
        {
            // Act
            input.ToRollingInterval();

            // Assert
            // Test is successful if no exception is thrown
        }
    }
}