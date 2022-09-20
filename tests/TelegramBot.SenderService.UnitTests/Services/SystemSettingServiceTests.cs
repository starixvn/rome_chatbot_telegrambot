using Fanex.Data.Repository;
using TelegramBot.SenderService.Criteria;
using TelegramBot.SenderService.Models;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.UnitTests.Services;

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
}