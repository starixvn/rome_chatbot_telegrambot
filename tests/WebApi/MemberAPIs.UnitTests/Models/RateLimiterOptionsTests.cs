using System;
using MemberAPIs.Models;
using Xunit;

namespace MemberAPIs.UnitTests.Models
{
    public class RateLimiterOptionsTests
    {
        [Fact]
        public void DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var options = new RateLimiterOptions();

            // Assert
            Assert.True(options.AutoReplenishment);
            Assert.Equal(250, options.PermitLimit);
            Assert.Equal(1, options.Window);
            Assert.Equal("s", options.Unit);
            Assert.Equal(1, options.SegmentsPerWindow);
        }

        [Theory]
        [InlineData("s", 1, 1)]
        [InlineData("m", 1, 60)]
        [InlineData("h", 1, 3600)]
        [InlineData("invalid", 1, 1)] // Should default to seconds for invalid unit
        public void GetWindowTimeSpan_ShouldReturnCorrectTimeSpan(string unit, int window, int expectedSeconds)
        {
            // Arrange
            var options = new RateLimiterOptions
            {
                Unit = unit,
                Window = window
            };

            // Act
            var timeSpan = options.GetWindowTimeSpan();

            // Assert
            Assert.Equal(expectedSeconds, timeSpan.TotalSeconds);
        }

        [Fact]
        public void CustomValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var options = new RateLimiterOptions
            {
                AutoReplenishment = false,
                PermitLimit = 100,
                Window = 5,
                Unit = "m",
                SegmentsPerWindow = 2
            };

            // Assert
            Assert.False(options.AutoReplenishment);
            Assert.Equal(100, options.PermitLimit);
            Assert.Equal(5, options.Window);
            Assert.Equal("m", options.Unit);
            Assert.Equal(2, options.SegmentsPerWindow);
        }
    }
}