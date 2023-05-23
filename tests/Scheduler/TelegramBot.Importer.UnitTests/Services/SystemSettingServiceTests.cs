using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.SystemSettings;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class SystemSettingServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly SystemSettingService systemSettingService;

    public SystemSettingServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        systemSettingService = new SystemSettingService(dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetSystemSetting_Always_CallsFetchAsync(string serviceName, string environment, IEnumerable<SystemSettingEntity> settings)
    {
        // Arrange
        dynamicRepository.FetchAsync<SystemSettingEntity>(Arg.Any<GetSystemSettingCriteria>()).Returns(Task.FromResult(settings));

        // Act
        await systemSettingService.GetSystemSetting(serviceName, environment);

        // Assert
        await dynamicRepository.Received().FetchAsync<SystemSettingEntity>(Arg.Is<GetSystemSettingCriteria>(c => c.ServiceName == serviceName && c.Environment == environment));
    }

    [Theory, AutoData]
    public async Task UpdateJobStatusAndLastRunTime_Always_CallsExecuteAsync(string serviceName, string environment, string jobStatus, DateTime lastRuntime)
    {
        // Act
        await systemSettingService.UpdateJobStatusAndLastRunTime(serviceName, environment, jobStatus, lastRuntime);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<UpdateSystemSettingCommand>(c => c.ServiceName == serviceName && c.Environment == environment));
    }
}