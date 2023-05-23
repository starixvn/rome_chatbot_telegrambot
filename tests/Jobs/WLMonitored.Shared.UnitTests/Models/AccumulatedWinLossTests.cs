using System;
using WLMonitored.Shared.Models;
using Xunit;

namespace WLMonitored.Shared.UnitTests.Criteria;

public class AccumulatedWinLossTests
{
    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange & Act
        var now = DateTime.Now;
        var criteria = new AccumulatedWinLoss
        {
            CustId = 123456,
            AccumWL = 100.78M,
            UpdatedTime = now
        };

        // Assert
        Assert.True(
            123456 == criteria.CustId
            && 100.78M == criteria.AccumWL
            && now == criteria.UpdatedTime
        );
    }
}