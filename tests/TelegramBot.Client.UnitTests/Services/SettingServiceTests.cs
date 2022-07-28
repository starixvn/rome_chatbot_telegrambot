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

public class SettingServiceTests
{
    [Theory, AutoData]
    public async Task GenerateStartUriAsync__Always_ReturnCorrectResult(int custId, int subAccId, string system, string language, string startUri)
    {
        //Arrange
        var httpClient = StubHttpClient(startUri);
        var settingService = new SettingService(httpClient);

        // Act
        var actualResult = await settingService.GenerateStartUriAsync(custId, subAccId, system, language);

        // Assert
        Assert.Equal(startUri, actualResult);
    }

    [Theory, AutoData]
    public async Task StartAsync__Always_ReturnCorrectResult(long messengerBotId, long chatId, string message, string ip)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(new Result(0));
        var httpClient = StubHttpClient(response);
        var settingService = new SettingService(httpClient);

        // Act
        var actualResult = await settingService.StartAsync(messengerBotId, chatId, message, ip);

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task ResendOtpAsync__Always_ReturnCorrectResult(int custId, int subAccId)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(new Result(0));
        var httpClient = StubHttpClient(response);
        var settingService = new SettingService(httpClient);

        // Act
        var actualResult = await settingService.ResendOtpAsync(custId, subAccId);

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task LinkAsync__Always_ReturnCorrectResult(int botId, int custId, int subAccId, string otp, string username, string ip)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(new Result(0));
        var httpClient = StubHttpClient(response);
        var settingService = new SettingService(httpClient);

        // Act
        var actualResult = await settingService.LinkAsync(botId, custId, subAccId, otp, username, ip);

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task UnlinkAsync__Always_ReturnCorrectResult(int custId, int subAccId, string ip)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(new Result(0));
        var httpClient = StubHttpClient(response);
        var settingService = new SettingService(httpClient);

        // Act
        var actualResult = await settingService.UnlinkAsync(custId, subAccId, ip);

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task UpdateUserFunction__Always_ReturnCorrectResult(int userId, int botId, Function function, int creator, string ip)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(new Result(0));
        var httpClient = StubHttpClient(response);
        var settingService = new SettingService(httpClient);

        // Act
        var actualResult = await settingService.UpdateUserFunction(userId, botId, function, creator, ip);

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task UpdateLanguage__Always_ReturnCorrectResult(int userId, int botId, string language, int creator, string ip)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(new Result(0));
        var httpClient = StubHttpClient(response);
        var settingService = new SettingService(httpClient);

        // Act
        var actualResult = await settingService.UpdateLanguage(userId, botId, language, creator, ip);

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task GetCustBotMessengerAsync__Always_ReturnCorrectResult(int custId, int subAccId, string language, CustBotMessenger messenger)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(messenger);
        var httpClient = StubHttpClient(response);
        var settingService = new SettingService(httpClient);

        // Act
        var actualResult = await settingService.GetCustBotMessengerAsync(custId, subAccId, language);

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }

    private static HttpClient StubHttpClient(string response)
    {
        var messageHandler = new MockHttpMessageHandler(response, HttpStatusCode.OK);
        return new HttpClient(messageHandler) { BaseAddress = new Uri("https://localhost") };
    }
}