using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Fanex.Data.Repository;
using NSubstitute;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Message.Criteria;
using TelegramBot.Infrastructure.Shared.Models;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Message;

public class MessageLoggerTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly MessageLogger messageLogger;

    public MessageLoggerTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        messageLogger = new MessageLogger(dynamicRepository);
    }

    [Theory, AutoData]
    public async Task LogMessage_Always_CallsExecuteAsync(int sender, int receiver, Messenger messenger, string message, byte status, short functionId)
    {
        // Act
        await messageLogger.LogMessage(sender, receiver, messenger, message, status, functionId);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Any<CustBotMessageInsCriteria>());
    }
}