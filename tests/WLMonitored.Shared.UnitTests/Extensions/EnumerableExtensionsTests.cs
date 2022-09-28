using System.Collections.Generic;
using System.Linq;
using WLMonitored.Shared.Extensions;
using Xunit;

namespace WLMonitored.Shared.UnitTests.Extensions;

public class EnumerableExtensionsTests
{
    [Fact]
    public void JoinWithComma_EmptyList_ReturnEmpty()
    {
        // Arrange
        var list = Enumerable.Empty<int>();

        // Act & Assert
        Assert.Empty(list.JoinWithComma());
    }

    [Fact]
    public void JoinWithComma_NotEmptyList_ReturnCorrectValue()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };

        // Act & Assert
        Assert.Equal("1,2,3,4,5", list.JoinWithComma());
    }
}