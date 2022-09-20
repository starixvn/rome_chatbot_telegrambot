using TelegramBot.Importer.Criteria.MemberWinLossLimits;

namespace TelegramBot.Importer.UnitTests.Criteria.MemberWinLossLimits;

public class GetMemberWinLossLimitCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(int batchSize, long lastLogId)
    {
        // Arrange
        var criteria = new GetMemberWinLossLimitCommand(batchSize, lastLogId);

        // Act & Assert
        Assert.Equal("Age_Chatbot_CustWinLossReachLimitSel", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void IsValid_InvalidBatchSize_ReturnsFalse(int batchSize)
    {
        // Arrange
        var criteria = new GetMemberWinLossLimitCommand(batchSize, 1);

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory, AutoData]
    public void IsValid_InvalidBatchSize_ReturnsTrue(int batchSize, long lastLogId)
    {
        // Arrange
        var criteria = new GetMemberWinLossLimitCommand(batchSize, lastLogId);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}