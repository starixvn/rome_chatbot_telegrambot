using WLMonitored.Shared.Configuration;
using Xunit;

namespace WLMonitored.Shared.UnitTests.Configuration;

public class ScheduleJobTests
{
    [Fact]
    public void Contructor_Always_AssignsCorrectProperties()
    {
        // Arrange & Act
        var scheduleJob = new ScheduleJob
        {
            Name = "NewJob",
            BatchSize = 100,
            DelayExecutionTimeInSeconds = 5,
            IsActive = true
        };

        // Assert
        Assert.True(
            "NewJob" == scheduleJob.Name
            && null == scheduleJob.Environment
            && 100 == scheduleJob.BatchSize
            && 5 == scheduleJob.DelayExecutionTimeInSeconds
            && scheduleJob.IsActive);
    }

    [Fact]
    public void JobInfo_UAT_ReturnsCorrectJobInfo()
    {
        // Arrange
        var scheduleJob = new ScheduleJob
        {
            Name = "NewJob",
            Environment = "UAT",
            BatchSize = 100,
            DelayExecutionTimeInSeconds = 5,
            IsActive = true
        };

        // Act
        var actual = scheduleJob.JobInfo;

        // Assert
        Assert.Equal("NewJob - UAT", actual);
    }

    [Fact]
    public void JobInfo_PROD_ReturnsCorrectJobInfo()
    {
        // Arrange
        var scheduleJob = new ScheduleJob
        {
            Name = "NewJob",
            BatchSize = 100,
            DelayExecutionTimeInSeconds = 5,
            IsActive = true
        };

        // Act
        var actual = scheduleJob.JobInfo;

        // Assert
        Assert.Equal("NewJob", actual);
    }
}