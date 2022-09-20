
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Newtonsoft.Json;
using TelegramBot.Client.Models;
using TelegramBot.Client.Services;
using Xunit;

namespace TelegramBot.Client.UnitTests.Services;

public class CommandServiceTests
{
    private readonly HttpClient httpClient;
    private readonly CommandService commandService;
    private readonly string Response = JsonConvert.SerializeObject(new Result(0));
    public CommandServiceTests()
    {
        var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
        httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://localhost") };
        commandService = new CommandService(httpClient);
    }

    [Theory, AutoData]
    public async Task HandleTextMessage_Always_ReturnCorrectResult(long messengerBotId, long chatId, string message)
    {
        // Act
        var actualResult = await commandService.HandleTextMessage(messengerBotId, chatId, message);

        // Assert
        Assert.Equal(Response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task HandleCallbackMessage_Always_ReturnCorrectResult(long messengerBotId, long chatId, int messageId, string callbackId, string message)
    {
        // Act
        var actualResult = await commandService.HandleCallbackMessage(messengerBotId, chatId, messageId, callbackId, message);

        // Assert
        Assert.Equal(Response, JsonConvert.SerializeObject(actualResult));
    }
}