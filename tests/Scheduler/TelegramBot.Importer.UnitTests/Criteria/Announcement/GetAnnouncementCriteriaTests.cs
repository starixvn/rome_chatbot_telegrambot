using TelegramBot.Importer.Criteria.Announcement;

namespace TelegramBot.Importer.UnitTests.Criteria.Announcement;

public class GetAnnouncementCriteriaTests
{
    [Theory, AutoData]
    public void IsValid_FromDateIsGreaterThanToDate_ReturnsFalse(string environment)
    {
        // Arrange
        var fromDate = DateTime.Now;
        var toDate = fromDate.AddDays(-1);
        var criteria = new GetAnnouncementCriteria(fromDate, toDate, environment);

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory, AutoData]
    public void IsValid_FromDateIsLessThanToDate_ReturnsFalse(string environment)
    {
        // Arrange
        var fromDate = DateTime.Now;
        var toDate = fromDate.AddDays(1);
        var criteria = new GetAnnouncementCriteria(fromDate, toDate, environment);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}