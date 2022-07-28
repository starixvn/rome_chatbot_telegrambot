using AutoFixture.Xunit2;
using TelegramBot.Client.Services;
using Xunit;
using TelegramBot.Client.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System;

namespace TelegramBot.Client.UnitTests.Services;

[Trait("Services", "Notification")]
public class NotificationServiceTests
{
    private readonly HttpClient httpClient;
    private readonly NotificationService notificationService;
    private readonly string Response = JsonConvert.SerializeObject(new Result(0));
    public NotificationServiceTests()
    {
        var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
        httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://localhost") };
        notificationService = new NotificationService(httpClient);
    }

    [Theory, AutoData]
    public async Task NotifyChangePassword_Always_ReturnCorrectResult(int custId, int subAccId)
    {
        // Act
        var actualResult = await notificationService.NotifyChangePassword(custId, subAccId);

        // Assert
        Assert.Equal(Response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task NotifyChangeSecurityCode_Always_ReturnCorrectResult(int custId, int subAccId)
    {
        // Act
        var actualResult = await notificationService.NotifyChangeSecurityCode(custId, subAccId);

        // Assert
        Assert.Equal(Response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task Unlink_Always_ReturnCorrectResult(int userId, long chatId, int botId, string language)
    {
        // Act
        var actualResult = await notificationService.Unlink(userId, chatId, botId, language);

        // Assert
        Assert.Equal(Response, JsonConvert.SerializeObject(actualResult));
    }
}