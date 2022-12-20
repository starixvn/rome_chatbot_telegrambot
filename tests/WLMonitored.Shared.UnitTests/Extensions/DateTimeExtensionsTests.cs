using System;
using WLMonitored.Shared.Extensions;
using Xunit;

namespace WLMonitored.Shared.UnitTests.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ToSimpleDate_Always_ReturnCorrectValue()
    {
        // Arrange
        var date = new DateTime(2022, 6, 10, 10, 28, 55);

        // Act & Assert
        Assert.Equal("06/10/2022", date.ToSimpleDate());
    }
}