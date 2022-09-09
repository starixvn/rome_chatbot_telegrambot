using TelegramBot.Importer.Criteria.ProblemAccounts;

namespace TelegramBot.Importer.UnitTests.Criteria.ProblemAccounts;

public class GetProblemAccountCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(int batchSize, int lastLogId)
    {
        // Arrange
        var criteria = new GetProblemAccountCommand(lastLogId, batchSize);

        // Act & Assert
        Assert.Equal("Age_Chatbot_ProblemAccounts_Sel", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void IsValid_InvalidBatchSize_ReturnsFalse(int batchSize)
    {
        // Arrange
        var criteria = new GetProblemAccountCommand(1, batchSize);

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory, AutoData]
    public void IsValid_InvalidBatchSize_ReturnsTrue(int batchSize, int lastLogId)
    {
        // Arrange
        var criteria = new GetProblemAccountCommand(lastLogId, batchSize);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}