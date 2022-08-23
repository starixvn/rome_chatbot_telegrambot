using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.PersonalMessages;
using TelegramBot.Importer.Models.PersonalMessages;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class PersonalMessageServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly PersonalMessageService personalMessageService;

    public PersonalMessageServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        personalMessageService = new PersonalMessageService(dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetNewMessages_Always_ReturnsPersonalMessages(int msgTargetId, int batchSize, bool isTest, IEnumerable<PersonalMessage> personalMessages)
    {
        // Arrange
        dynamicRepository.FetchAsync<PersonalMessage>(Arg.Any<GetPersonalMessageCommand>()).Returns(Task.FromResult(personalMessages));

        // Act
        var result = await personalMessageService.GetNewMessages(msgTargetId, batchSize, isTest);

        // Assert
        Assert.Same(result.Item2, personalMessages);
    }

    [Theory, AutoData]
    public async Task Import_Always_CallsExecuteAsync(IEnumerable<PersonalMessageNotification> notifications, string environment, int maxMsgTargetId)
    {
        // Act
        await personalMessageService.Import(notifications, environment, maxMsgTargetId);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<ImportPersonalMessageCommand>(c => c.Environment == environment && c.MaxMsgTargetId == maxMsgTargetId));
    }
}