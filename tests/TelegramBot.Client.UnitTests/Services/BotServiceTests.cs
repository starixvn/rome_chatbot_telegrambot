using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Newtonsoft.Json;
using TelegramBot.Client.Models;
using TelegramBot.Client.Services;
using Xunit;

namespace TelegramBot.Client.UnitTests.Services;

public class BotServiceTests
{
    [Theory, AutoData]
    public async Task GetAllAsync_Always_ReturnCorrectResult(IEnumerable<BotEntity> botEntities)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(botEntities);
        var messageHandler = new MockHttpMessageHandler(response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://localhost") };
        var botService = new BotService(httpClient);

        // Act
        var actualResult = await botService.GetAllAsync();

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }

    [Theory, AutoData]
    public async Task GetBySystem_Always_ReturnCorrectResult(string system, BotEntity bot)
    {
        //Arrange
        var response = JsonConvert.SerializeObject(bot);
        var messageHandler = new MockHttpMessageHandler(response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://localhost") };
        var botService = new BotService(httpClient);

        // Act
        var actualResult = await botService.GetBySystem(system);

        // Assert
        Assert.Equal(response, JsonConvert.SerializeObject(actualResult));
    }
}